using Dargon.Nest.Internals.Deployment;

namespace Dargon.Nest {
   public interface ReadableDeployment : ReadableDeploymentMetadata, ReadableBundleContainer {
      ReadableDeploymentMetadata Metadata { get; }
      ReadableBundleContainer BundleContainer { get; }
   }
}