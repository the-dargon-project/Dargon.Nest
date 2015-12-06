using System.Collections.Generic;

namespace Dargon.Nest.Internals.Bundles {
   public interface ReadableBundleMetadata {
      string Channel { get; }
      string Remote { get; }
      string Version { get; }
   }

   public interface ReadableEggContainer {
      string Location { get; }
      IEnumerable<ReadableEgg> EnumerateEggs();
   }

   public interface ManageableEggContainer : ReadableEggContainer {
      new IEnumerable<ManageableEgg> EnumerateEggs();
   }
}
