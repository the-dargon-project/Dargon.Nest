using System.Collections.Generic;

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
      public string Channel => Metadata.Channel;
      public string Remote => Metadata.Remote;
      public string Version => Metadata.Version;

      // ReadableEggContainer
      public string Location => EggContainer.Location;
      IEnumerable<ReadableEgg> ReadableEggContainer.EnumerateEggs() => EnumerateEggs();

      // ManageableBundle
      public ManageableEggContainer EggContainer { get; }

      // ManageableEggContainer
      public IEnumerable<ManageableEgg> EnumerateEggs() => EggContainer.EnumerateEggs();
   }
}
