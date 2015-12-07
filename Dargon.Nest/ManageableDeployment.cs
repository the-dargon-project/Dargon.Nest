using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Dargon.Nest.Internals.Deployment;

namespace Dargon.Nest {
   public interface ManageableDeployment : ReadableDeployment, ManageableBundleContainer {
      new ManageableBundleContainer BundleContainer { get; }
   }
}
