using System;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Deployment;
using Fody.Constructors;
using ItzWarty;
using ItzWarty.Collections;
using Nito.AsyncEx;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Restart;
using NLog;
using SCG = System.Collections.Generic;

namespace Dargon.Nest.Daemon.Hatchlings {
   [RequiredFieldsConstructor]
   public class HatchlingSpawnerServiceImpl {
      private readonly HatchlingSpawner hatchlingSpawner = null;

      public async Task<SpawnHatchlingResult> SpawnHatchlingAsync(string eggName, SpawnConfiguration configuration) {
         var hatchling = hatchlingSpawner.Spawn(eggName, configuration);

         var result = new SpawnHatchlingResult();
         result.HatchlingId = hatchling.Id;
         if (!configuration.StartFlags.HasFlag(HatchlingStartFlags.StartAsynchronously)) {
            result.StartResult = await hatchling.GetStartResultAsync();
         }
         return result;
      }
   }

   [RequiredFieldsConstructor]
   public class HatchlingDirectoryServiceImpl {
      private readonly ManageableHatchlingDirectory hatchlingDirectory = null;

      public async Task<SCG.IEnumerable<HatchlingStateDto>> EnumerateHatchlingsAsync() {
         var result = new SCG.List<HatchlingStateDto>();
         foreach (var hatchling in hatchlingDirectory.EnumerateHatchlings()) {
            result.Add(await hatchling.ToDataTransferObjectAsync());
         }
         return result;
      }
   }

   [RequiredFieldsConstructor]
   public class HatchlingKillerServiceImpl {
      private readonly ManageableHatchlingDirectory hatchlingDirectory = null;

      public Task KillAllHatchlingsAsync(ShutdownReason reason) {
         return KillHatchlingsAsync(reason, hatchlingDirectory.EnumerateHatchlings());
      }

      public Task KillHatchlingsOfBundlesAsync(ShutdownReason reason, params string[] bundleNames) {
         var hatchlings = bundleNames.SelectMany(hatchlingDirectory.EnumerateHatchlingsOfBundle);
         return KillHatchlingsAsync(reason, hatchlings);
      }

      public Task KillHatchlingsAsync(ShutdownReason reason, SCG.IEnumerable<HatchlingContext> hatchlings) {
         return Task.WhenAll(hatchlings.Select(x => x.ShutdownAsync(reason)));
      }
   }

   [RequiredFieldsConstructor]
   public class HatchlingPatcherServiceImpl {
      private readonly AsyncLock synchronization = new AsyncLock();
      private readonly ManageableDeployment deployment = null;
      private readonly HatchlingPatcherWorkerImpl patcherWorker = null;

      public async Task RunPatchCycleAsync() {
         using (await synchronization.LockAsync()) {
            var initEgg = EggFactory.Local(IoUtilities.CombinePath(deployment.Location, "init"));
            await initEgg.SyncAsync();

            var latestRemote = await deployment.GetLatestRemoteDeploymentAsync();
            var updateAvailability = DeploymentOperations.GetUpdateAvailability(deployment, latestRemote);
            if (updateAvailability != UpdateAvailability.None) {
               await patcherWorker.PatchToDeploymentAsync(latestRemote);
            }
         }
      }
   }

   [RequiredFieldsConstructor]
   public class HatchlingPatcherWorkerImpl {
      private static readonly TimeSpan postHatchlingKillCommitAttemptDuration = TimeSpan.FromSeconds(5);
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly ExeggutorSynchronization synchronization = null;
      private readonly NestDaemonService nestDaemonService = null;
      private readonly HatchlingKillerServiceImpl hatchlingKillerService = null;
      private readonly RestartSignalService restartSignalService = null;
      private readonly ManageableDeployment deployment = null;

      public async Task PatchToDeploymentAsync(ReadableDeployment latestRemote) {
         logger.Info($"{nameof(PatchToDeploymentAsync)} to remote {latestRemote.Name} {latestRemote.Remote} {latestRemote.Version}");

         using (await synchronization.TakeExclusiveLockAsync())
         using (var updateContext = new DeploymentUpdateContext(deployment)) {
            try {
               logger.Info("Beginning update process...");
               updateContext.BeginUpdate();

               logger.Info("Setting remote deployment...");
               updateContext.SetRemoteDeployment(latestRemote);

               logger.Info("Staging updates...");
               await updateContext.StageUpdatesAsync();
               var dryCommitSuccessful = true;
               try {
                  updateContext.CommitUpdates(dryRun: true);
                  logger.Info($"Trial dry commit succeeded!");
               } catch (Exception e) {
                  logger.Info($"Trial dry commit failed...", e);
                  dryCommitSuccessful = false;
               }
               if (dryCommitSuccessful) {
                  logger.Info($"Committing updates...");
                  updateContext.CommitUpdates(dryRun: false);
               } else {
                  var stagedBundleNames = new HashSet<string>(updateContext.EnumerateStagedBundleNames());
                  logger.Info($"Staged bundle names: " + stagedBundleNames.Join(", "));
                  if (!stagedBundleNames.Contains("nest")) {
                     logger.Info($"Staged bundle names don't include nest. Killing hatchlings of staged bundles...");
                     await hatchlingKillerService.KillHatchlingsOfBundlesAsync(ShutdownReason.Update, stagedBundleNames.ToArray());

                     // dry run until success
                     logger.Info($"Preparing to commit updates (via test dry runs)...");
                     var startTime = DateTime.Now;
                     while (DateTime.Now - startTime < postHatchlingKillCommitAttemptDuration) {
                        try {
                           logger.Info($"Running pre-commit dry run.");
                           updateContext.CommitUpdates(dryRun: true);
                           break;
                        } catch (Exception e) {
                           logger.Info($"Dry run of commit failed.", e);
                        }
                     }
                     logger.Info($"Committing updates for real...");
                     updateContext.CommitUpdates(dryRun: true);

                     logger.Info($"Processing restart signals...");
                     restartSignalService.ProcessRestartSignals();
                  } else {
                     logger.Info($"Staged bundle names includes nest... Killing everything.");
                     await hatchlingKillerService.KillAllHatchlingsAsync(ShutdownReason.Update);

                     logger.Info($"Requesting nest daemon restart.");
                     nestDaemonService.RestartDaemon();
                  }
               }
            } finally {
               updateContext.EndUpdate();
            }
         }
      }
   }
}
