using Dargon.Management;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using Fody.Constructors;
using ItzWarty;
using System.Linq;

namespace Dargon.Nest.Daemon {
   [RequiredFieldsConstructor]
   public class ExeggutorMob {
      private readonly ReadableHatchlingDirectory hatchlingDirectory = null;
      private readonly BundleDirectoryImpl bundleDirectory = null;
      private readonly HatchlingSpawnerServiceImpl hatchlingSpawnerService = null;
      private readonly HatchlingKillerServiceImpl hatchlingKillerService = null;
      private readonly HatchlingPatcherServiceImpl hatchlingPatcherService = null;
      private readonly ManageableDeployment localDeployment = null;

      [ManagedProperty]
      public string DeploymentLocation => localDeployment.Location;

      [ManagedProperty]
      public string DeploymentName => localDeployment.Name;

      [ManagedProperty]
      public string DeploymentChannel => localDeployment.Channel;

      [ManagedProperty]
      public string DeploymentRemote => localDeployment.Remote;

      [ManagedProperty]
      public string DeploymentVersion => localDeployment.Version;

      [ManagedOperation]
      public string EnumerateNests() {
         return bundleDirectory.EnumerateBundles().Select(x => x.Name).Join("\r\n");
      }

      [ManagedOperation]
      public string EnumerateHatchlings() {
         return hatchlingDirectory.EnumerateHatchlings().Select(x => $"{x.Id.ToString("N")} {x.Name}").Join("\r\n");
      }

      [ManagedOperation]
      public string SpawnHatchling(string eggName, string instanceName) {
         SpawnConfiguration configuration = new SpawnConfiguration { Arguments = null, InstanceName = instanceName };
         var spawnResult = hatchlingSpawnerService.SpawnHatchlingAsync(eggName, configuration).Result;
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
      public string KillHatchlingsForUpdate(string name) {
         HatchlingContext hatchling;
         if (!hatchlingDirectory.TryGetHatchlingByName(name, out hatchling)) {
            return $"Couldn't find hatchling of name \"{name}\"";
         }
         hatchling.ShutdownAsync(ShutdownReason.Update);
         return $"Sent shutdown command to hatchling of name \"{name}\"!";
      }

      [ManagedOperation]
      public string KillHatchlingsAndUpdateAllPackages() {
         hatchlingKillerService.KillAllHatchlingsAsync(ShutdownReason.None).Wait();
         return "Success!";
      }

      [ManagedOperation]
      public string CheckForUpdates() {
         hatchlingPatcherService.RunPatchCycleAsync().Wait();
         return "Success!";
      }
   }
}
