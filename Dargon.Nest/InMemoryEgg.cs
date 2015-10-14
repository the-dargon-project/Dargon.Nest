using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public class InMemoryEgg : IDargonEgg {
      private readonly Dictionary<string, EggFileListEntry> entriesByInternalPath = new Dictionary<string, EggFileListEntry>();
      private readonly string name;
      private readonly string version;
      private readonly string sourcePath;

      public InMemoryEgg(string name, string version, string sourcePath) {
         this.name = name;
         this.version = version;
         this.sourcePath = FormatSystemPath(sourcePath);
         var internalFilePaths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).Select(GetInternalPath).ToArray();
         foreach (var internalPath in internalFilePaths) {
            var hash = NestUtil.GetFileHash(GetAbsolutePath(internalPath));
            var entry = new EggFileListEntry(hash, internalPath);
            entriesByInternalPath.Add(internalPath, entry);
         }
      }

      public string Name { get { return name; } }
      public string Location { get { return "(in-memory)"; } }
      public string Version { get { return version; } }
      public string Remote { get { return ""; } }

      public IReadOnlyList<EggFileListEntry> Files { get { return entriesByInternalPath.Values.ToArray(); } }

      public Stream GetStream(string internalPath) {
         return File.Open(GetAbsolutePath(internalPath), FileMode.Open, FileAccess.Read, FileShare.Read);
      }

      public bool IsValid() {
         return true;
      }
      
      private string FormatSystemPath(string absolutePath) {
         return Path.GetFullPath(absolutePath).TrimEnd('/', '\\');
      }

      private string GetAbsolutePath(string internalPath) {
         return Path.Combine(sourcePath, internalPath);
      }

      private string GetInternalPath(string absolutePath) {
         const int kLeadingSlashLength = 1;
         return FormatSystemPath(absolutePath).Substring(sourcePath.Length + kLeadingSlashLength);
      }
   }
}
