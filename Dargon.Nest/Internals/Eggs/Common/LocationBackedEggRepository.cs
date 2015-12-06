using System;
using System.Collections.Generic;

namespace Dargon.Nest.Internals.Eggs.Common {
   public class LocationBackedEggRepository : ManageableEggRepository {
      private readonly string path;

      public LocationBackedEggRepository(string path) {
         this.path = path;
      }

      public string Location => path;

      public IEnumerable<EggFileEntry> EnumerateFiles() {
         var data = IoUtilities.ReadString(ComputeFullPath("filelist"));
         return EggFileEntrySerializer.Deserialize(data);
      }

      public string ComputeFullPath(string internalPath) {
         return IoUtilities.CombinePath(path, internalPath);
      }

      public void Sync(ReadableEggRepository remote) {
         if (!IoUtilities.IsLocal(path)) {
            throw new NotSupportedException("Cannot sync as path is not local: " + path);
         }
         EggOperations.Update(path, remote);
      }
   }
}