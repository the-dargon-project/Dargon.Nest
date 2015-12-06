using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Repl {
   public class ListEggsCommand : ICommand {
      public string Name { get { return "list-eggs"; } }

      public int Eval(string args) {
         var nest = new LocalDargonNest(ReplGlobals.CurrentDirectory);
         var eggs = nest.EnumerateEggs().ToArray();
         if (eggs.Length == 0) {
            Console.WriteLine("The nest is empty.");
         }
         foreach (var egg in eggs) {
            Console.WriteLine(egg.Name + " " + egg.Version);
         }
         return 0;
      }
   }
}
