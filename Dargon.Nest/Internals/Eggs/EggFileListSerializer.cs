using System;
using System.Collections.Generic;
using System.Text;

namespace Dargon.Nest.Internals.Eggs {
   public static class EggFileEntrySerializer {
      public static IReadOnlyList<EggFileEntry> Deserialize(string content) {
         content = content.Trim();
         var lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
         var result = new EggFileEntry[lines.Length];
         for (var i = 0; i < lines.Length; i++) {
            result[i] = ParseFileListEntry(lines[i]);
         }
         return result;
      }

      public static string Serialize(IEnumerable<EggFileEntry> entries) {
         var result = new StringBuilder();
         var isFirstEntry = true;
         foreach (var entry in entries) { 
            if (isFirstEntry) {
               isFirstEntry = false;
            } else {
               result.AppendLine();
            }
            result.Append(entry.Guid.ToString("n") + " " + entry.InternalPath);
         }
         return result.ToString();
      }

      private static EggFileEntry ParseFileListEntry(string s) {
         s = s.Trim();
         var firstSpaceIndex = s.IndexOf(' ');
         var guid = Guid.Parse(s.Substring(0, firstSpaceIndex));
         var internalPath = s.Substring(firstSpaceIndex + 1);
         return new EggFileEntry(guid, internalPath);
      }
   }
}