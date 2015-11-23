using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;
using Fody.Constructors;
using NLog;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface HatchlingSpawner {
      HatchlingContext Spawn(string eggName, SpawnConfiguration spawnConfiguration = null);
   }

   [RequiredFieldsConstructor]
   public class HatchlingSpawnerImpl : HatchlingSpawner {
      private static Logger logger = LogManager.GetCurrentClassLogger();
      private readonly HostOperations hostOperations = null;
      private readonly HostProcessFactory hostProcessFactory = null;
      private readonly EggDirectory eggDirectory = null;
      private readonly ManageableHatchlingDirectory hatchlingDirectory = null;

      public HatchlingContext Spawn(string eggName, SpawnConfiguration spawnConfiguration = null) {
         var hatchlingId = Guid.NewGuid();
         var eggContext = eggDirectory.GetContextByName(eggName);

         logger.Info($"Spawning instance of egg {eggName} with id {hatchlingId}!");
         spawnConfiguration = spawnConfiguration ?? new SpawnConfiguration();
         spawnConfiguration.Arguments = spawnConfiguration.Arguments ?? new byte[0];
         spawnConfiguration.InstanceName = spawnConfiguration.InstanceName ?? (eggContext.Name + "-" + hatchlingId.ToString("n"));

         hostOperations.CopyHostToLocalEgg(eggContext.Egg);
         var hostProcess = hostProcessFactory.CreateAndInitialize(eggContext, spawnConfiguration);
         var hatchlingContext = new HatchlingContextImpl(hatchlingId, hostProcess, spawnConfiguration, eggContext, hatchlingDirectory);
         hatchlingContext.Initialize();
         return hatchlingContext;
      }
   }
}