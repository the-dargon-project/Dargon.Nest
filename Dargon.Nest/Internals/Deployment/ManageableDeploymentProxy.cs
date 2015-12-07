using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Deployment {
   public class ManageableDeploymentProxy : ManageableDeployment {
      public ManageableDeploymentProxy(ReadableDeploymentMetadata metadata, ManageableBundleContainer bundleContainer) {
         Metadata = metadata;
         BundleContainer = bundleContainer;
      }

      // ReadableDeployment
      public ReadableDeploymentMetadata Metadata { get; }
      ReadableBundleContainer ReadableDeployment.BundleContainer => BundleContainer;

      // ReadableDeploymentMetadata 
      public string Name => Metadata.Name;
      public string Version => Metadata.Version;
      public string Remote => Metadata.Remote;
      public string Channel => Metadata.Channel;

      // ReadableBundleContainer
      async Task<IEnumerable<ReadableBundle>> ReadableBundleContainer.EnumerateBundlesAsync() => await EnumerateBundlesAsync();
      public string Location => BundleContainer.Location;

      // ManageableDeployment
      public ManageableBundleContainer BundleContainer { get; }
      public Task<IEnumerable<ManageableBundle>> EnumerateBundlesAsync() => BundleContainer.EnumerateBundlesAsync();
   }
}