using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Daemon.Updating;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using Fody.Constructors;
using Nito.AsyncEx;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Nest.Daemon {
   [RequiredFieldsConstructor]
   public class NestServiceImpl : ExeggutorService {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      // Nonrecursive; reads for single-nest operations, write for multi-nest operations.
      private readonly AsyncReaderWriterLock multinestAccessLock = new AsyncReaderWriterLock();
      private readonly HatchlingSpawner hatchlingSpawner = null;
      private readonly ManageableHatchlingDirectory hatchlingDirectory = null;
      private readonly ManageableNestDirectory nestDirectory = null;
      private readonly UpdateFetcher updateFetcher = null;
      private readonly StagedUpdateProcessor stagedUpdateProcessor = null;

      public SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration) {
         return SpawnHatchlingAsync(eggName, configuration).Result;
      }

      public async Task<SpawnHatchlingResult> SpawnHatchlingAsync(string eggName, SpawnConfiguration configuration) {
         using (await multinestAccessLock.ReaderLockAsync()) {
            var hatchling = hatchlingSpawner.Spawn(eggName, configuration);

            var result = new SpawnHatchlingResult();
            result.HatchlingId = hatchling.Id;
            if (!configuration.StartFlags.HasFlag(HatchlingStartFlags.StartAsynchronously)) {
               result.StartResult = hatchling.StartResult;
            }
            return result;
         }
      }

      public IEnumerable<HatchlingStateDto> EnumerateHatchlings() {
         return EnumerateHatchlingsAsync().Result;
      }

      public async Task<IEnumerable<HatchlingStateDto>> EnumerateHatchlingsAsync() {
         using (await multinestAccessLock.ReaderLockAsync()) {
            return from hatchling in hatchlingDirectory.EnumerateHatchlings()
                   select hatchling.ToDataTransferObject();
         }
      }

      public void KillHatchlings() {
         KillHatchlingsAsync().Wait();
      }

      public async Task KillHatchlingsAsync() {
         logger.Info("Invoking KillHatchlings!");
         using (await multinestAccessLock.WriterLockAsync()) {
            await KillHatchlings_UnderLockAsync(ShutdownReason.None);
         }
      }

      public void KillHatchlingsAndUpdateAllPackages() {
         KillHatchlingsAndUpdateAllPackagesAsync().Wait();
      }

      public async Task KillHatchlingsAndUpdateAllPackagesAsync() {
         logger.Info("Invoking KillHatchlingsAndUpdateAllPackages!");
         using (await multinestAccessLock.WriterLockAsync()) {
            await KillHatchlings_UnderLockAsync(ShutdownReason.Update);
            await UpdateHatchlings_UnderLockAsync();
         }
      }

      private Task KillHatchlings_UnderLockAsync(ShutdownReason reason) {
         return KillHatchlings_UnderLockAsync(reason, hatchlingDirectory.EnumerateHatchlings());
      }

      private Task KillHatchlings_UnderLockAsync(ShutdownReason reason, IEnumerable<HatchlingContext> hatchlings) {
         return Task.WhenAll(hatchlings.Select(x => x.ShutdownAsync(reason)));
      }

      private async Task UpdateHatchlings_UnderLockAsync() {
         await updateFetcher.FetchUpdatesAsync();
         foreach (var nest in nestDirectory.EnumerateNests()) {
            if (stagedUpdateProcessor.IsUpdateStaged(nest.Name)) {
               stagedUpdateProcessor.ProcessDeadNestWithStagedUpdate(nest.Name);
            }
         }
      }
   }
}
