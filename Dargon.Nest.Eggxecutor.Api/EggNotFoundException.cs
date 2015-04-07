using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Eggxecutor {
   public class EggNotFoundException : Exception {
      public EggNotFoundException(
         string eggName
      ) : base(FormatMessage(eggName)) {
      }

      private static string FormatMessage(string eggName) {
         return "Could not find Nest Egg '" + eggName + "'";
      }
   }
}
