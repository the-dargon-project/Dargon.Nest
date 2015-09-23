using System;
using System.Collections.Specialized;
using System.Linq;
using Dargon.Management;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Egg;
using Dargon.Nest.Eggxecutor;
using ItzWarty;

namespace Dargon.Nest.Daemon {
   public class ExeggutorMob {
      private readonly InternalNestDaemonService daemonService;
      private readonly ReadableHatchlingDirectory hatchlingDirectory;
      private readonly NestDirectory nestDirectory;
      private readonly HarglBarglImpl harglBargl;

      public ExeggutorMob(InternalNestDaemonService daemonService, ReadableHatchlingDirectory hatchlingDirectory, NestDirectory nestDirectory, HarglBarglImpl harglBargl) {
         this.daemonService = daemonService;
         this.hatchlingDirectory = hatchlingDirectory;
         this.nestDirectory = nestDirectory;
         this.harglBargl = harglBargl;
      }

      [ManagedOperation]
      public string EnumerateNests() {
         return nestDirectory.EnumerateNests().Select(x => x.Name).Join("\r\n");
      }

      [ManagedOperation]
      public string EnumerateHatchlings() {
         return hatchlingDirectory.EnumerateHatchlings().Select(x => $"{x.Id.ToString("N")} {x.Name}").Join("\r\n");
      }

      [ManagedOperation]
      public string SpawnHatchling(string eggName, string instanceName) {
         SpawnConfiguration configuration = new SpawnConfiguration { Arguments = null, InstanceName = instanceName };
         var spawnResult = harglBargl.SpawnHatchling(eggName, configuration);
         return "Spawned hatchling of guid " + spawnResult.HatchlingId + ": " + spawnResult.StartResult;
      }

      [ManagedOperation]
      public string KillHatchlingByName(string name) {
         HatchlingContext hatchling;
         if (!hatchlingDirectory.TryGetHatchlingByName(name, out hatchling)) {
            return $"Couldn't find hatchling of name \"{name}\"";
         }
         hatchling.Shutdown(ShutdownReason.None);
         return $"Sent shutdown command to hatchling of name \"{name}\"!";
      }

      [ManagedOperation]
      public string KillHatchlingsAndUpdateAllPackages() {
         harglBargl.KillHatchlingsAndUpdateAllPackages();
         return "Success!";
      }
   }
}
