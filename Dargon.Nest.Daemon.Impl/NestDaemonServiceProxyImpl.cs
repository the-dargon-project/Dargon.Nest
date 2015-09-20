using System;
using System.Threading;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;

namespace Dargon.Nest.Daemon {
   public class NestDaemonServiceProxyImpl : NestDaemonService, ExeggutorService {
      private readonly HatchlingSpawner hatchlingSpawner;
      public NestDaemonServiceProxyImpl(HatchlingSpawner hatchlingSpawner) {
         this.hatchlingSpawner = hatchlingSpawner;
      }

      public SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration) {
         return hatchlingSpawner.Spawn(eggName, configuration).SpawnResult;
      }

      public void KillAllHatchlingsAndUpdateAllPackages() {
         throw new NotImplementedException();
      }

      public void KillHatchlingsAndDaemon() {
         throw new NotImplementedException();
      }
   }
}
