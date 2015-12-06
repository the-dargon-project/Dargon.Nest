using System.Collections.Generic;
using System.IO;
using Dargon.Nest.Internals.Eggs;

namespace Dargon.Nest.Internals.Bundles.Local {
   public class LocalLocationBackedEggContainer : ManageableEggContainer {
      private readonly string location;

      public LocalLocationBackedEggContainer(string location) {
         this.location = location;
      }

      public string Location => location;

      public IEnumerable<ManageableEgg> EnumerateEggs() {
         foreach (var directory in Directory.EnumerateDirectories(location)) {
            yield return EggFactory.FileBacked(directory);
         }
      }

      IEnumerable<ReadableEgg> ReadableEggContainer.EnumerateEggs() {
         return EnumerateEggs();
      }
   }
}