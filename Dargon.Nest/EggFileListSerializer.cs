using System;
using System.Collections.Generic;
using System.Text;

namespace Dargon.Nest {
   public static class EggFileListSerializer {
      public static IReadOnlyList<EggFileListEntry> Deserialize(string content) {
         content = content.Trim();
         var lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
         var result = new EggFileListEntry[lines.Length];
         for (var i = 0; i < lines.Length; i++) {
            result[i] = ParseFileListEntry(lines[i]);
         }
         return result;
      }

      public static string Serialize(IReadOnlyList<EggFileListEntry> entries) {
         StringBuilder result = new StringBuilder();
         for (var i = 0; i < entries.Count; i++) {
            if (i != 0) {
               result.AppendLine();
            }
            result.Append(entries[i].Guid.ToString("n") + " " + entries[i].InternalPath);
         }
         return result.ToString();
      }

      private static EggFileListEntry ParseFileListEntry(string s) {
         s = s.Trim();
         var firstSpaceIndex = s.IndexOf(' ');
         var guid = Guid.Parse(s.Substring(0, firstSpaceIndex));
         var internalPath = s.Substring(firstSpaceIndex + 1);
         return new EggFileListEntry(guid, internalPath);
      }
   }
}