using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public class EggFileListEntry {
      private readonly Guid guid;
      private readonly string internalPath;

      public EggFileListEntry(Guid guid, string internalPath) {
         this.guid = guid;
         this.internalPath = internalPath;
      }

      public Guid Guid { get { return guid; } }
      public string InternalPath { get { return internalPath; } }
   }
}
