using System;
using System.Collections.Generic;

namespace Dargon.Nest.Internals.Eggs.InMemory {
   public class InMemoryEggRepository : ManageableEggRepository {
      private readonly string path;
      private readonly IEnumerable<EggFileEntry> entries;

      public InMemoryEggRepository(string path, IEnumerable<EggFileEntry> entries) {
         this.path = path;
         this.entries = entries;
      }

      public string Location => path;
      public IEnumerable<EggFileEntry> EnumerateFiles() => entries;
      public string ComputeFullPath(string internalPath) => IoUtilities.CombinePath(path, internalPath);
      public void Sync(ReadableEggRepository remote) {
         throw new NotSupportedException("Cannot sync in-memory egg.");
      }
   }
}
