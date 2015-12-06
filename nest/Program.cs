using Dargon.Repl;
using ItzWarty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace Dargon.Nest.Repl {
   public static class Program {
      public static int Main(string[] args) {
         args = ProcessArguments(args);

         var dispatcher = new DispatcherCommand("command_root");
         dispatcher.RegisterCommand(new ExitCommand());

         if (ReplGlobals.InteractiveMode) {
            return new ReplCore(dispatcher).Run();
         } else {
            return dispatcher.Eval(string.Join(" ", args));
         }
      }

      private static string[] ProcessArguments(string[] args) {
         while (args.Length > 0 && args.First()[0] == '-') {
            switch (args.First()) {
               case "-i":
                  ReplGlobals.InteractiveMode = true;
                  args = args.SubArray(1);
                  break;
               case "-d":
                  ReplGlobals.CurrentDirectory = args[1];
                  args = args.SubArray(2);
                  break;
               default:
                  Console.WriteLine("Unknown argument: " + args[0]);
                  break;
            }
         }
         return args;
      }
   }
}
