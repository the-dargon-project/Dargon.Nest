using System;
using Dargon.Management;
using ItzWarty;
using System.Linq;
using Dargon.Nest.Eggxecutor;

namespace Dargon.Nest.Exeggutor {
   public class ExeggutorMob {
      private readonly ExeggutorServiceImpl exeggutorService;

      public ExeggutorMob(ExeggutorServiceImpl exeggutorService) {
         this.exeggutorService = exeggutorService;
      }

      [ManagedOperation]
      public string EnumerateHatchlings() {
         return exeggutorService.Hatchlings.Select(x => $"{x.InstanceId.ToString("N")} {x.Name}").Join("\r\n");
      }

      [ManagedOperation]
      public string SpawnHatchling(string eggName, string instanceName) {
         SpawnConfiguration configuration = new SpawnConfiguration { Arguments = null, InstanceName = instanceName };
         var hatchlingGuid = exeggutorService.SpawnHatchling(eggName, configuration);
         return "Spawned hatchling of guid " + hatchlingGuid;
      }

      [ManagedOperation]
      public string KillHatchlingByName(string name) {
         var hatchling = exeggutorService.Hatchlings.FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
         if (hatchling == null) {
            return $"Couldn't find hatchling of name \"{name}\"";
         } else {
            hatchling.Shutdown();
            return $"Sent shutdown command to hatchling of name \"{name}\"!";
         }
      }
   }
}
