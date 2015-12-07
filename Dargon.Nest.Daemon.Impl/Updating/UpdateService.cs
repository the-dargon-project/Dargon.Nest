using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Deployment;
using Fody.Constructors;
using ItzWarty;
using ItzWarty.Collections;
using System.IO;
using System.Net;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggs;

namespace Dargon.Nest.Daemon.Updating {
   public interface UpdateService {
      void RunPeriodicUpdateCheck();
   }

   [RequiredFieldsConstructor]
   public class UpdateServiceImpl : UpdateService {
      private readonly object synchronization = new object();
      private readonly WebClient webClient = new WebClient();
      private readonly ExeggutorServiceImpl exeggutorService = null;
      private readonly NestDaemonServiceImpl nestDaemonService = null;
      private readonly ManageableDeployment deployment = null;

      public void RunPeriodicUpdateCheck() {
         lock (synchronization) {
            var initEgg = EggFactory.Local(IoUtilities.CombinePath(deployment.Location, "init"));
            initEgg.Sync();

            var latestRemote = deployment.GetLatestRemoteDeploymentAsync();
            var updateAvailability = DeploymentOperations.GetUpdateAvailability(deployment, latestRemote);
            if (updateAvailability == UpdateAvailability.None) return;
            using(var updateContext = new DeploymentUpdateContext(deployment)) {
               try {
                  updateContext.BeginUpdate();
                  updateContext.SetRemoteDeployment(latestRemote);
                  updateContext.StageUpdatesAsync();
                  var dryCommitSuccessful = Util.IsThrown<IOException>(() => updateContext.CommitUpdates(dryRun: true));
                  if (dryCommitSuccessful) {
                     updateContext.CommitUpdates(dryRun: false);
                  } else {
                     var stagedBundleNames = new HashSet<string>(updateContext.EnumerateStagedBundleNames());
                     if (!stagedBundleNames.Contains("nest")) {
                        exeggutorService.KillHatchlingsOfBundlesAndExecAsync(
                           ShutdownReason.Update,
                           stagedBundleNames,
                           () => {
                              updateContext.CommitUpdates();
                           }).Wait();
                     } else {
                        exeggutorService.KillHatchlingsOfBundlesAndExecAsync(
                           ShutdownReason.Update,
                           stagedBundleNames,
                           () => {
                              nestDaemonService.RestartDaemonViaInit();
                           }).Wait();
                     }
                  }
               } finally {
                  updateContext.EndUpdate();
               }
            }
         }
      }
   }
}
