using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Eggxecutor;
using NLog;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface HatchlingSpawner {
      HatchlingContext Spawn(string eggFullName, SpawnConfiguration spawnConfiguration);
   }

   public class HatchlingSpawnerImpl : HatchlingSpawner {
      private static Logger logger = LogManager.GetCurrentClassLogger();
      private readonly DaemonConfiguration daemonConfiguration;
      private readonly HostOperations hostOperations;

      public HatchlingSpawnerImpl(DaemonConfiguration daemonConfiguration, HostOperations hostOperations) {
         this.daemonConfiguration = daemonConfiguration;
         this.hostOperations = hostOperations;
      }

      public HatchlingContext Spawn(string eggFullName, SpawnConfiguration spawnConfiguration) {
         Guid hatchlingId = Guid.NewGuid();

         logger.Info($"Spawning instance of egg {eggFullName} with id {hatchlingId}!");
         spawnConfiguration = spawnConfiguration ?? new SpawnConfiguration();
         spawnConfiguration.Arguments = spawnConfiguration.Arguments ?? new byte[0];
         spawnConfiguration.InstanceName = spawnConfiguration.InstanceName ?? hatchlingId.ToString("n");

         var eggNameParts = eggFullName.Split('/');
         var nestName = eggNameParts[0];
         var eggName = eggNameParts[1];

         var nest = new LocalDargonNest(Path.Combine(daemonConfiguration.NestsPath, nestName));
         var egg = (LocalDargonEgg)nest.EnumerateEggs().First(e => e.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
         hostOperations.StartHostProcess(egg, $"--name {spawnConfiguration.InstanceName}");
      }
   }
}