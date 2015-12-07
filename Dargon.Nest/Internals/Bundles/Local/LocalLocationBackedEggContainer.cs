using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dargon.Nest.Internals.Eggs;

namespace Dargon.Nest.Internals.Bundles.Local {
   public class LocalLocationBackedEggContainer : ManageableEggContainer {
      private readonly string location;

      public LocalLocationBackedEggContainer(string location) {
         this.location = location;
      }

      public string Location => location;

      public Task<IEnumerable<ManageableEgg>> EnumerateEggsAsync() => Task.FromResult(EnumerateEggsHelper());

      private IEnumerable<ManageableEgg> EnumerateEggsHelper() {
         foreach (var directory in Directory.EnumerateDirectories(location)) {
            yield return EggFactory.Local(directory);
         }
      }

      async Task<IEnumerable<ReadableEgg>> ReadableEggContainer.EnumerateEggsAsync() => await EnumerateEggsAsync();
   }
}