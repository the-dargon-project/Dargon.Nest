using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Repl {
   public class UpdateNestCommand : ICommand {
      private readonly WebClient webClient = new WebClient();

      public string Name { get { return "update-nest"; } }

      public int Eval(string args) {
         var nest = new LocalDargonNest(ReplGlobals.CurrentDirectory);
         nest.UpdateNest();
         return 0;
      }
   }
}
