using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggxecutor;
using Fody.Constructors;
using NLog;

namespace Dargon.Nest.Daemon.Init.Handlers {
   [RequiredFieldsConstructor]
   public class RunEggInitScriptActionHandlerImpl : InitScriptActionHandler {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly HatchlingSpawner hatchlingSpawner = null;

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
