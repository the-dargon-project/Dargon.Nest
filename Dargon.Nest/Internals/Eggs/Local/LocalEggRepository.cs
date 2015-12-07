using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Eggs.Local {
   public class LocalEggRepository : ManageableEggRepository {
      private readonly string path;

      public LocalEggRepository(string path) {
         this.path = path;
      }

      public string Location => path;

      public async Task<IEnumerable<EggFileEntry>> EnumerateFilesAsync() {
         var data = await IoUtilities.ReadStringAsync(ComputeFullPath(NestConstants.kFileListFileName));
         return EggFileEntrySerializer.Deserialize(data);
      }

      public string ComputeFullPath(string internalPath) {
         return IoUtilities.CombinePath(path, internalPath);
      }

      public Task SyncAsync(ReadableEggRepository remote) {
         if (!IoUtilities.IsLocal(path)) {
            throw new NotSupportedException("Cannot sync as path is not local: " + path);
         }
         return EggOperations.UpdateAsync(path, EggFactory.FromPath(remote.Location));
      }
   }
}