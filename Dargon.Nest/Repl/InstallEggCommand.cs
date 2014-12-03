using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Repl {
   public class InstallEggCommand : DispatcherCommand {
      private const string kCommandName = "install-egg";

      public InstallEggCommand() : base(kCommandName) {
         RegisterCommand(new RemoteCommand());
         RegisterCommand(new LocalCommand());
      }

      public class RemoteCommand : ICommand {
         public string Name { get { return "remote"; } }

         public int Eval(string args) {
            var sourceEgg = RemoteDargonEgg.FromUrl(args);
            var localNest = new LocalDargonNest(ReplGlobals.NestPath);
            localNest.InstallEgg(sourceEgg);
            return 0;
         }
      }
   }

   public class LocalCommand : ICommand {
      public string Name { get { return "local"; } }

      public int Eval(string args) {
         args = args.Trim();
         
         if (Directory.Exists(args)) {
            var sourceEgg = new LocalDargonEgg(args);
            if (!sourceEgg.IsValid()) {
               Console.Error.WriteLine("Did not find valid egg at path \"" + args + "\".");
               return 1;
            } else {
               var localNest = new LocalDargonNest(ReplGlobals.NestPath);
               localNest.InstallEgg(sourceEgg);
               return 0;
            }
         } else {
            Console.Error.WriteLine("Did not find directory at path \"" + args + "\".");
            return 2;
         }
      }
   }
}
