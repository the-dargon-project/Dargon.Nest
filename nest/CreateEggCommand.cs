using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;

namespace Dargon.Nest.Repl {
   public class CreateEggCommand : ICommand {
      public string Name { get { return "create-egg"; } }

      public int Eval(string args) {
         string name, version;
         args = Util.NextToken(args, out name);
         args = Util.NextToken(args, out version);
         var path = Path.GetFullPath(args.Trim());

         var inMemoryEgg = new InMemoryEgg(name, version, path);
         var nest = new LocalDargonNest(ReplGlobals.NestPath);
         nest.InstallEgg(inMemoryEgg);
         return 0;
      }
   }
}
