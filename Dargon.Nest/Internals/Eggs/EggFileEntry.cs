using System;

namespace Dargon.Nest.Internals.Eggs {
   public class EggFileEntry {
      public EggFileEntry(Guid guid, string internalPath) {
         Guid = guid;
         InternalPath = internalPath;
      }

      public Guid Guid { get; }
      public string InternalPath { get; }
   }
}