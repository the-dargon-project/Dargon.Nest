using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Internals.Nests;
using Dargon.Nest.Internals.Nests.Local;

namespace Dargon.Nest {
   public static class NestFactory {
      public static ManageableNest Local(string path) {
         return new ManageableNestProxy(
            new LocalDeploymentContainerImpl(path));
      }
   }
}
