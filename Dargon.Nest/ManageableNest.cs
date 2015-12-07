using Dargon.Nest.Internals.Deployment;
using Dargon.Nest.Internals.Nests;

namespace Dargon.Nest {
   public interface ManageableNest : ReadableNest, ManageableDeploymentContainer {
      new ManageableDeploymentContainer DeploymentContainer { get; }
   }
}