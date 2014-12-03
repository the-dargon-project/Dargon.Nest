using System;
using System.Linq;

namespace Dargon.Nest.Repl {
   public static class Program {
      public static int Main(string[] argsArray) {
         var args = string.Join(" ", argsArray);
         var argsTokens = Util.QASS(args);
         while (argsTokens.Length > 0) {
            if (argsTokens[0][0] == '-') {
               switch (argsTokens[0]) {
                  case "--nest-path":
                     ReplGlobals.NestPath = argsTokens[1];
                     if (ReplGlobals.NestPath == ".") {
                        ReplGlobals.NestPath = Environment.CurrentDirectory;
                     }
                     argsTokens = argsTokens.Skip(2).ToArray();
                     break;
                  default:
                     Console.Error.WriteLine("Unknown argument " + argsTokens[0] + ".");
                     return 1;
               }
            } else {
               break;
            }
         }

         var dispatcher = new DispatcherCommand("command_root");
         dispatcher.RegisterCommand(new ListEggsCommand());
         dispatcher.RegisterCommand(new InstallEggCommand());
         dispatcher.RegisterCommand(new CreateEggCommand());
         dispatcher.RegisterCommand(new UpdateEggCommand());
         dispatcher.RegisterCommand(new ExitCommand());

         if (argsTokens.Length > 0) {
            return dispatcher.Eval(string.Join(" ", argsTokens));
         }

         if (ReplGlobals.NestPath == null) {
            Console.Write("Nest Path: ");
            ReplGlobals.NestPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ReplGlobals.NestPath)) {
               ReplGlobals.NestPath = "C:/Dargon";
               Console.WriteLine("Using default nest path C:/Dargon");
            }
            Console.WriteLine();
         }

         while (true) {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) {
               try {
                  dispatcher.Eval(input);
               } catch (Exception e) {
                  Console.Error.WriteLine(e.Message);
                  Console.Error.WriteLine(e.StackTrace);
               }
               Console.WriteLine();
            }
         }
      }
   }
}
