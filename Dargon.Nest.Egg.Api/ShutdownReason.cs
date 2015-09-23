using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Egg {
   public enum ShutdownReason : int {
      None = 0,
      HostKilled = 1,
      Update = 2
   }
}
