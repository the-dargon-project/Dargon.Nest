using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Nests {
   public interface ReadableDeploymentContainer {
      string Location { get; }
      IEnumerable<ReadableDeployment> EnumerateDeployments();
   }
}
