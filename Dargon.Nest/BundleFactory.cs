using Dargon.Nest.Internals.Bundles;
using Dargon.Nest.Internals.Bundles.Local;

namespace Dargon.Nest {
   public static class BundleFactory {
      public static ManageableBundle Local(string location) {
         return new ManageableBundleProxy(
            new LocalLocationBackedBundleMetadata(location),
            new LocalLocationBackedEggContainer(location));
      }
   }
}
