using ItzWarty;

namespace Dargon.Nest.Repl {
   public class ExecEggCommand : ICommand {
      public string Name { get { return "exec"; } }

      public int Eval(string args) {
         string eggName;
         args = Util.NextToken(args, out eggName);

         var nest = new LocalDargonNest(ReplGlobals.NestPath);
         return nest.ExecuteEgg(eggName, args);
      }
   }
}
