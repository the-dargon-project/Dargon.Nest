using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggxecutor;
using NLog;

namespace Dargon.Nest.Daemon.Init.Handlers {
   public class RunEggInitScriptActionHandlerImpl : InitScriptActionHandler {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly HatchlingSpawner hatchlingSpawner;

      public RunEggInitScriptActionHandlerImpl(HatchlingSpawner hatchlingSpawner) {
         this.hatchlingSpawner = hatchlingSpawner;
      }

      public string ActionName => "run-egg";

      public void Process(NestContext nest, dynamic action) {
         string eggName = action["egg-name"];
         string instanceName = action["instance-name"] ?? eggName;
         hatchlingSpawner.Spawn(eggName, new SpawnConfiguration {
            InstanceName = instanceName
         });
      }
   }
}
