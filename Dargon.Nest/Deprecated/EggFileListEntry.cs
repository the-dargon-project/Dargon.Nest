using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public class EggFileListEntry {
      public EggFileListEntry(Guid guid, string internalPath) {
         Guid = guid;
         InternalPath = internalPath;
      }

      public Guid Guid { get; }
      public string InternalPath { get; }
   }
}
