using System.Collections.Generic;
using System.IO;
using Dargon.Nest.Internals.Deployment;
using Dargon.Nest.Internals.Nests;

namespace Dargon.Nest {
   public interface ReadableNest : ReadableDeploymentContainer {
      ReadableDeploymentContainer DeploymentContainer { get; }
   }
}