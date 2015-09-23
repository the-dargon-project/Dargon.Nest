using System;
using System.Threading;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;

namespace Dargon.Nest.Daemon {
   public class HarglBarglImpl : ExeggutorService {
      private readonly HatchlingSpawner hatchlingSpawner;

      public HarglBarglImpl(HatchlingSpawner hatchlingSpawner) {
         this.hatchlingSpawner = hatchlingSpawner;
      }

      public SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration) {
         var hatchling = hatchlingSpawner.Spawn(eggName, configuration);

         var result = new SpawnHatchlingResult();
         result.HatchlingId = hatchling.Id;
         if (!configuration.StartFlags.HasFlag(HatchlingStartFlags.StartAsynchronously)) {
            result.StartResult = hatchling.StartResult;
         }
         return result;
      }

      public void KillHatchlings() {
         throw new NotImplementedException();
      }

      public void KillHatchlingsAndUpdateAllPackages() {
         throw new NotImplementedException();
      }
   }
}
