using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dargon.Nest {
   public class LocalDargonEgg : IDargonEgg {
      private readonly string rootPath;
      private readonly DirectoryInfo directoryInfo;
      private readonly string versionFilePath;
      private readonly string fileListPath;
      private readonly string remoteFilePath;

      public LocalDargonEgg(string directory) {
         directoryInfo = new DirectoryInfo(directory);
         rootPath = directoryInfo.FullName;

         versionFilePath = Path.Combine(rootPath, NestConstants.kVersionFileName);
         fileListPath = Path.Combine(rootPath, NestConstants.kFileListFileName);
         remoteFilePath = Path.Combine(rootPath, NestConstants.kRemoteFileName);
      }

      public string Name { get { return directoryInfo.Name; } }
      public string Location { get { return rootPath; } }
      public string Version { get { return File.ReadAllText(versionFilePath); } }
      public string Remote { get { return File.ReadAllText(remoteFilePath); } }

      public IReadOnlyList<EggFileListEntry> Files {
         get {
            var content = File.ReadAllText(fileListPath);
            return EggFileListSerializer.Deserialize(content);
         }
      }

      public Stream GetStream(string internalPath) {
         var path = Path.Combine(rootPath, internalPath);
         return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      }

      public bool IsValid() {
         return File.Exists(versionFilePath) && File.Exists(fileListPath);
      }
   }
}
