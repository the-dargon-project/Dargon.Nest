using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Deployment {
   public class ReadableDeploymentProxy : ReadableDeployment {
      public ReadableDeploymentProxy(ReadableDeploymentMetadata metadata, ReadableBundleContainer bundleContainer) {
         Metadata = metadata;
         BundleContainer = bundleContainer;
      }

      // ReadableDeployment
      public ReadableDeploymentMetadata Metadata { get; }
      public ReadableBundleContainer BundleContainer { get; }

      // ReadableDeploymentMetadata 
      public string Name => Metadata.Name;
      public string Version => Metadata.Version;
      public string Remote => Metadata.Remote;
      public string Channel => Metadata.Channel;

      // ReadableBundleContainer
      public string Location => BundleContainer.Location;
      public Task<IEnumerable<ReadableBundle>> EnumerateBundlesAsync() => BundleContainer.EnumerateBundlesAsync();
   }
}