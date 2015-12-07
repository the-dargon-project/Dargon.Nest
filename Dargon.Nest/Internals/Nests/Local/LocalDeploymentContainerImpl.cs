using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Nests.Local {
   public class LocalDeploymentContainerImpl : ManageableDeploymentContainer {
      public LocalDeploymentContainerImpl(string location) {
         Location = location;
      }

      public string Location { get; }

      public IEnumerable<ManageableDeployment> EnumerateDeployments() {
         var deploymentsPath = IoUtilities.CombinePath(Location, NestConstants.kDeploymentsDirectoryName);
         IoUtilities.PrepareDirectory(deploymentsPath);
         foreach (var directoryPath in Directory.EnumerateDirectories(deploymentsPath)) {
            yield return DeploymentFactory.Local(directoryPath);
         }
      }

      IEnumerable<ReadableDeployment> ReadableDeploymentContainer.EnumerateDeployments() => EnumerateDeployments();
   }
}
