using Dargon.Nest.Eggxecutor;
using NLog;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface HatchlingSpawner {
      HatchlingContext Spawn(string eggFullName, SpawnConfiguration spawnConfiguration);
   }

   public class HatchlingSpawnerImpl : HatchlingSpawner {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      public HatchlingContext Spawn(string eggFullName, SpawnConfiguration configuration) {
         logger.Info($"Spawning instance of egg {eggFullName}!");
         configuration = configuration ?? new SpawnConfiguration();
         configuration.Arguments = configuration.Arguments ?? new byte[0];
      }
   }
}