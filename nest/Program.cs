using Dargon.Repl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dargon.Nest.Repl {
   public static class Program {
      public static int Main(string[] argsArray) {
         ReplGlobals.NestPath = Environment.CurrentDirectory;

         var tokenQueue = new Queue<string>();
         var freeTokens = new List<string>();
         foreach (var token in string.Join(" ", argsArray).QASS()) {
            var equalsIndex = token.IndexOf("=");
            if (token.StartsWith("--") && equalsIndex != -1) {
               var property = token.Substring(0, equalsIndex);
               var value = token.Substring(equalsIndex + 1);
               tokenQueue.Enqueue(property);
               tokenQueue.Enqueue(value);
            } else {
               tokenQueue.Enqueue(token);
            }
         }

         Dictionary<char, Action> flagHandlers = new Dictionary<char, Action> {
            { 'i', () => ReplGlobals.InteractiveMode = true }
         };

         Dictionary<string, Action<Queue<string>>> switchHandlers = new Dictionary<string, Action<Queue<string>>> {
            { "interactive", (q) => ReplGlobals.InteractiveMode = true },
            { "nest-path", (q) => ReplGlobals.NestPath = q.Dequeue() }
         };

         while (tokenQueue.Any()) {
            var token = tokenQueue.Dequeue();
            if (!token.StartsWith("-")) {
               freeTokens.Add(token);
            } else {
               if (token.StartsWith("--")) {
                  switchHandlers[token.Substring(2)](tokenQueue);
               } else {
                  for (var i = 1; i < token.Length; i++) {
                     flagHandlers[token[i]]();
                  }
               }
            }
         }

         ReplGlobals.NestPath = Path.GetFullPath(ReplGlobals.NestPath);

         var dispatcher = new DispatcherCommand("command_root");
         dispatcher.RegisterCommand(new UpdateNestCommand());
         dispatcher.RegisterCommand(new ListEggsCommand());
         dispatcher.RegisterCommand(new InstallEggCommand());
         dispatcher.RegisterCommand(new CreateEggCommand());
         dispatcher.RegisterCommand(new UpdateEggCommand());
         dispatcher.RegisterCommand(new ExecEggCommand());
         dispatcher.RegisterCommand(new ExitCommand());

         if (freeTokens.Count > 0) {
            return dispatcher.Eval(string.Join(" ", freeTokens));
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

         var fileInfo = new FileInfo(ReplGlobals.NestPath);
         if (fileInfo.Exists) {
            Console.Error.WriteLine("The nest path is not a directory!");
            return -1;
         } else if(!fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
            Console.WriteLine("Creating Nest at path `{0}`", ReplGlobals.NestPath);
            NestUtil.PrepareDirectory(ReplGlobals.NestPath);
         }

         return new ReplCore(dispatcher).Run();
      }
   }
}
