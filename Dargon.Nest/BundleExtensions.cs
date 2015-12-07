using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Bundles;
using Dargon.Nest.Internals.Deployment;
using Dargon.Nest.Internals.Eggs;
using Dargon.Nest.Internals.Nests;
using Semver;

namespace Dargon.Nest {
   public static class EggExtensions {
      public static Task SyncAsync(this ManageableEgg egg) {
         return egg.SyncAsync(EggFactory.FromPath(egg.Remote));
      }
   }

   public static class BundleExtensions {
      public static bool ContainsEgg(this ReadableEggContainer container, string eggName) {
         ReadableEgg eggThrowaway;
         return TryGetEgg(container, eggName, out eggThrowaway);
      }

      public static ReadableEgg GetEgg(this ReadableEggContainer container, string eggName) {
         ReadableEgg result;
         if (!container.TryGetEgg(eggName, out result)) {
            throw new KeyNotFoundException($"Could not find egg of name `{eggName}`.");
         }
         return result;
      }

      public static ManageableEgg GetEgg(this ManageableEggContainer container, string eggName) {
         ManageableEgg result;
         if (!container.TryGetEgg(eggName, out result)) {
            throw new KeyNotFoundException($"Could not find egg of name `{eggName}`.");
         }
         return result;
      }

      public static ReadableEgg GetEggOrNull(this ReadableEggContainer container, string eggName) {
         ReadableEgg result;
         container.TryGetEgg(eggName, out result);
         return result;
      }

      public static ManageableEgg GetEggOrNull(this ManageableEggContainer container, string eggName) {
         ManageableEgg result;
         container.TryGetEgg(eggName, out result);
         return result;
      }

      public static bool TryGetEgg(this ReadableEggContainer container, string eggName, out ReadableEgg egg) {
         egg = container.EnumerateEggsAsync().Result.FirstOrDefault(x => x.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
         return egg != null;
      }

      public static bool TryGetEgg(this ManageableEggContainer container, string eggName, out ManageableEgg egg) {
         egg = container.EnumerateEggsAsync().Result.FirstOrDefault(x => x.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
         return egg != null;
      }

      public static Task InstallEggAsync(this ManageableEggContainer container, ReadableEgg remoteEgg) {
         ManageableEgg existingEgg;
         if (container.TryGetEgg(remoteEgg.Name, out existingEgg)) {
            return existingEgg.SyncAsync(remoteEgg);
         } else {
            return EggOperations.InstallAsync(Path.Combine(container.Location, remoteEgg.Name), remoteEgg);
         }
      }

      public static Task SyncEggAsync(this ManageableEggContainer container, string eggName) {
         return container.GetEgg(eggName).SyncAsync();
      }

      public static async Task SyncAsync(this ManageableEggContainer container) {
         var eggs = await container.EnumerateEggsAsync();
         var syncOperations = eggs.Select(e => e.SyncAsync());
         foreach (var task in syncOperations) {
            await task;
         }
      }
   }

   public enum UpdateAvailability {
      None = 0,
      Patch = 1,
      Minor = 2,
      Major = 3
   }

   public static class DeploymentExtensions {
      public static Task<ReadableDeployment> GetLatestRemoteDeploymentAsync(this ReadableDeployment deployment) {
         return DeploymentFactory.RemoteLatestAsync(deployment.Remote, deployment.Name, deployment.Channel);
      }
   }

   public static class NestExtensions {
      public static bool TryGetDeployment(this ManageableDeploymentContainer container, string deploymentName, out ManageableDeployment deployment) {
         deployment = container.EnumerateDeployments().FirstOrDefault(x => x.Name.Equals(deploymentName, StringComparison.OrdinalIgnoreCase));
         return deployment != null;
      }

      public static bool TryGetDeployment(this ReadableDeploymentContainer container, string deploymentName, out ReadableDeployment deployment) {
         deployment = container.EnumerateDeployments().FirstOrDefault(x => x.Name.Equals(deploymentName, StringComparison.OrdinalIgnoreCase));
         return deployment != null;
      }

      public static Task InstallDeploymentAsync(this ManageableDeploymentContainer container, ReadableDeployment remoteDeployment) {
         var destinationDeploymentDirectory = Path.Combine(container.Location, NestConstants.kDeploymentsDirectoryName, remoteDeployment.Name);
         ManageableDeployment existingDeployment;
         if (container.TryGetDeployment(remoteDeployment.Name, out existingDeployment)) {
            return DeploymentOperations.UpdateAsync(destinationDeploymentDirectory, remoteDeployment);
         } else {
            return DeploymentOperations.InstallAsync(destinationDeploymentDirectory, remoteDeployment);
         }
      }
   }
}