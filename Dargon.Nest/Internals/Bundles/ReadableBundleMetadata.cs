using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Bundles {
   public interface ReadableBundleMetadata {
      string Name { get; }
      string Version { get; }
      string Remote { get; }
      string InitScript { get; }
   }

   public interface ReadableEggContainer {
      string Location { get; }
      Task<IEnumerable<ReadableEgg>> EnumerateEggsAsync();
   }

   public interface ManageableEggContainer : ReadableEggContainer {
      new Task<IEnumerable<ManageableEgg>> EnumerateEggsAsync();
   }
}
