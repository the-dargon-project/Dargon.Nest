using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Bundles {
   public class ManageableBundleProxy : ManageableBundle {
      public ManageableBundleProxy(ReadableBundleMetadata metadata, ManageableEggContainer eggContainer) {
         Metadata = metadata;
         EggContainer = eggContainer;
      }

      // ReadableBundle
      public ReadableBundleMetadata Metadata { get; }
      ReadableEggContainer ReadableBundle.EggContainer => EggContainer;

      // ReadableBundleMetadata
      public string Name => Metadata.Name;
      public string Remote => Metadata.Remote;
      public string Version => Metadata.Version;
      public string InitScript => Metadata.InitScript;

      // ReadableEggContainer
      public string Location => EggContainer.Location;
      async Task<IEnumerable<ReadableEgg>> ReadableEggContainer.EnumerateEggsAsync() => await EnumerateEggsAsync();

      // ManageableBundle
      public ManageableEggContainer EggContainer { get; }

      // ManageableEggContainer
      public Task<IEnumerable<ManageableEgg>> EnumerateEggsAsync() => EggContainer.EnumerateEggsAsync();
   }
}
