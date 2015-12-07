using Dargon.Nest.Daemon.Hatchlings;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Dargon.Nest.Daemon.Init {
   public interface InitScriptRunner {
      void RunNestInitializationScript(BundleContext bundle);
   }

   public class InitScriptRunnerImpl : InitScriptRunner {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly ReadableBundleDirectory bundleDirectory;
      private readonly IReadOnlyDictionary<string, InitScriptActionHandler> actionHandlersByType;

      public InitScriptRunnerImpl(ReadableBundleDirectory bundleDirectory, IReadOnlyDictionary<string, InitScriptActionHandler> actionHandlersByType) {
         this.bundleDirectory = bundleDirectory;
         this.actionHandlersByType = actionHandlersByType;
      }

      public void RunNestInitializationScript(BundleContext bundle) {
         var initFilePath = Path.Combine(bundle.Path, "init.json");
         if (!File.Exists(initFilePath)) return;
         logger.Info(initFilePath);
         var initFileJson = File.ReadAllText(initFilePath);
         dynamic initConfig = JsonConvert.DeserializeObject(initFileJson);
         if (initConfig == null) return; // happens on empty init file
         var initHooks = initConfig["init-hooks"];
         foreach (dynamic action in initHooks) {
            string actionType = action.type;
            InitScriptActionHandler handler;
            if (!actionHandlersByType.TryGetValue(actionType, out handler)) {
               logger.Error("Could not find init script handler for type " + action.type);
            } else {
               handler.Process(bundle, action);
            }
         }
      }
   }

   public interface InitScriptActionHandler {
      string ActionName { get; }
      void Process(BundleContext bundle, dynamic action);
   }
}
