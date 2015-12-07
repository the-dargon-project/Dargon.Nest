using System.Collections.Generic;

namespace Dargon.Nest.Internals.Nests {
   public interface ManageableDeploymentContainer : ReadableDeploymentContainer {
      new IEnumerable<ManageableDeployment> EnumerateDeployments();
   }
}