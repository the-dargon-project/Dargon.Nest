using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Bundles {
   public class ReadableBundleProxy : ReadableBundle {
      public ReadableBundleProxy(ReadableBundleMetadata metadata, ReadableEggContainer eggContainer) {
         Metadata = metadata;
         EggContainer = eggContainer;
      }

      // ReadableBundle
      public ReadableBundleMetadata Metadata { get; }
      public ReadableEggContainer EggContainer { get; }

      // ReadableBundleMetadata
      public string Name => Metadata.Name;
      public string Remote => Metadata.Remote;
      public string Version => Metadata.Version;
      public string InitScript => Metadata.InitScript;

      // ReadableEggContainer
      public string Location => EggContainer.Location;
      public Task<IEnumerable<ReadableEgg>> EnumerateEggsAsync() => EggContainer.EnumerateEggsAsync();
   }
}