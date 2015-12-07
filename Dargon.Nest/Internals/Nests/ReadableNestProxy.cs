using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Nests {
   public class ManageableNestProxy : ManageableNest {
      public ManageableNestProxy(ManageableDeploymentContainer deploymentContainer) {
         DeploymentContainer = deploymentContainer;
      }

      // ReadableNest
      ReadableDeploymentContainer ReadableNest.DeploymentContainer => DeploymentContainer;

      // ReadableDeploymentContainer
      public string Location => DeploymentContainer.Location;
      IEnumerable<ReadableDeployment> ReadableDeploymentContainer.EnumerateDeployments() => EnumerateDeployments();

      // ManageableNest
      public ManageableDeploymentContainer DeploymentContainer { get; }

      // ManageableDeploymentContainer
      public IEnumerable<ManageableDeployment> EnumerateDeployments() => DeploymentContainer.EnumerateDeployments();
   }
}
