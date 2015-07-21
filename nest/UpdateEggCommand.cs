using ItzWarty;

namespace Dargon.Nest.Repl {
   public class UpdateEggCommand : ICommand {
      public string Name { get { return "update-egg"; } }

      public int Eval(string args) {
         string eggName;
         args = Util.NextToken(args, out eggName);
         var localNest = new LocalDargonNest(ReplGlobals.NestPath);
         localNest.UpdateEgg(eggName);
         return 0;
      }
   }
}
