using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Bundles;
using Dargon.Nest.Internals.Bundles.Local;
using Dargon.Nest.Internals.Bundles.Remote;

namespace Dargon.Nest {
   public static class BundleFactory {
      public static ManageableBundle Local(string location) {
         return new ManageableBundleProxy(
            new LocalLocationBackedBundleMetadata(location),
            new LocalLocationBackedEggContainer(location));
      }

      public static ReadableBundle Remote(string name, string version, string remote) {
         var location = IoUtilities.CombinePath(remote, NestConstants.kBundlesDirectoryName, name, NestConstants.kReleasesDirectoryName, $"{name}-{version}");
         return new ReadableBundleProxy(
            new RemoteUrlBackedBundleMetadata(name, version, remote, location),
            new RemoteUrlBackedEggContainer(name, version, remote, location));
      }
   }
}
