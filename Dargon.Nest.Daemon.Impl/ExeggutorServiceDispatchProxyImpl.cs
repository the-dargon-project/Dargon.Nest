using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using Fody.Constructors;

namespace Dargon.Nest.Daemon {
   [RequiredFieldsConstructor]
   public class ExeggutorServiceDispatchProxyImpl : ExeggutorService {
      private readonly HatchlingSpawnerServiceImpl hatchlingSpawnerService = null;
      private readonly HatchlingDirectoryServiceImpl hatchlingDirectoryService = null;
      private readonly HatchlingKillerServiceImpl hatchlingKillerService = null;
      private readonly HatchlingPatcherServiceImpl hatchlingPatcherService = null;

      public SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration) {
         return SpawnHatchlingAsync(eggName, configuration).Result;
      }

      public Task<SpawnHatchlingResult> SpawnHatchlingAsync(string eggName, SpawnConfiguration configuration) {
         return hatchlingSpawnerService.SpawnHatchlingAsync(eggName, configuration);
      }

      public IEnumerable<HatchlingStateDto> EnumerateHatchlings() {
         return EnumerateHatchlingsAsync().Result;
      }

      public Task<IEnumerable<HatchlingStateDto>> EnumerateHatchlingsAsync() {
         return hatchlingDirectoryService.EnumerateHatchlingsAsync();
      }

      public void KillAllHatchlings(ShutdownReason reason) {
         KillAllHatchlingsAsync(reason).Wait();
      }

      public Task KillAllHatchlingsAsync(ShutdownReason reason) {
         return hatchlingKillerService.KillAllHatchlingsAsync(reason);
      }

      public void KillHatchlingsOfBundles(ShutdownReason reason, params string[] bundleNames) {
         KillHatchlingsOfBundlesAsync(reason, bundleNames).Wait();
      }

      public Task KillHatchlingsOfBundlesAsync(ShutdownReason reason, params string[] bundleNames) {
         return hatchlingKillerService.KillHatchlingsOfBundlesAsync(reason, bundleNames);
      }

      public void RunUpdateCycle() {
         RunUpdateCycleAsync().Wait();
      }

      public Task RunUpdateCycleAsync() {
         return hatchlingPatcherService.RunPatchCycleAsync();
      }
   }
}