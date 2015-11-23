using Dargon.Management;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Egg;
using Dargon.Nest.Eggxecutor;
using Fody.Constructors;
using ItzWarty;
using System.Linq;

namespace Dargon.Nest.Daemon {
   [RequiredFieldsConstructor]
   public class ExeggutorMob {
      private readonly ReadableHatchlingDirectory hatchlingDirectory = null;
      private readonly NestDirectoryImpl nestDirectory = null;
      private readonly NestServiceImpl nestService = null;

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
         var spawnResult = nestService.SpawnHatchling(eggName, configuration);
         return "Spawned hatchling of guid " + spawnResult.HatchlingId + ": " + spawnResult.StartResult;
      }

      [ManagedOperation]
      public string KillHatchlingByName(string name) {
         HatchlingContext hatchling;
         if (!hatchlingDirectory.TryGetHatchlingByName(name, out hatchling)) {
            return $"Couldn't find hatchling of name \"{name}\"";
         }
         hatchling.ShutdownAsync(ShutdownReason.None);
         return $"Sent shutdown command to hatchling of name \"{name}\"!";
      }

      [ManagedOperation]
      public string KillHatchlingsAndUpdateAllPackages() {
         nestService.KillHatchlingsAndUpdateAllPackages();
         return "Success!";
      }
   }
}
