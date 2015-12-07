using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Dargon.Nest.Internals.Bundles;
using Dargon.Nest.Internals.Eggs;
using Dargon.Nest.Internals.Utilities;
using Semver;

namespace Dargon.Nest.Internals.Deployment {
   public class DeploymentUpdateContext : IDisposable {
      private readonly ManageableDeployment localDeployment;
      private readonly string stageDirectory;
      private readonly NestFileCache cache;
      private ReadableDeployment remoteDeployment;
      private IDisposable deploymentLock;

      public DeploymentUpdateContext(ManageableDeployment localDeployment) {
         this.localDeployment = localDeployment;

         var cachePath = IoUtilities.CombinePath(localDeployment.Location, NestConstants.kCacheDirectoryName);
         stageDirectory = IoUtilities.CombinePath(localDeployment.Location, NestConstants.kStageDirectoryName);
         cache = new NestFileCache(cachePath);
      }

      public async Task<UpdateAvailability> CheckForUpdatesAsync() {
         remoteDeployment = await localDeployment.GetLatestRemoteDeploymentAsync();
         return DeploymentOperations.GetUpdateAvailability(localDeployment, remoteDeployment);
      }

      public void SetRemoteDeployment(ReadableDeployment remote) {
         this.remoteDeployment = remote;
      }

      public void BeginUpdate() {
         deploymentLock = FileLock.Take(IoUtilities.CombinePath(localDeployment.Location, NestConstants.kLockFileName));
      }

      public async Task StageUpdatesAsync() {
         // sync cache
         var bundles = await remoteDeployment.EnumerateBundlesAsync();
         var eggEnumerateOperations = bundles.Select(b => b.EnumerateEggsAsync()).ToArray();
         var eggEnumerationResults = new List<ReadableEgg>();
         foreach (var eggEnumerationOperation in eggEnumerateOperations) {
            eggEnumerationResults.AddRange(await eggEnumerationOperation);
         }

         var eggFileListFetchingTasks = new List<Tuple<ReadableEgg, Task<IEnumerable<EggFileEntry>>>>();
         foreach (var egg in eggEnumerationResults) {
            eggFileListFetchingTasks.Add(new Tuple<ReadableEgg, Task<IEnumerable<EggFileEntry>>>(egg, egg.EnumerateFilesAsync()));
         }

         var eggsAndFiles = new List<Tuple<ReadableEgg, EggFileEntry>>();
         foreach (var eggFileListFetchingTask in eggFileListFetchingTasks) {
            var egg = eggFileListFetchingTask.Item1;
            var files = await eggFileListFetchingTask.Item2;
            foreach (var file in files) {
               eggsAndFiles.Add(new Tuple<ReadableEgg, EggFileEntry>(egg, file));
            }
         }

         var filesToPull = from eggsAndFile in eggsAndFiles
                           let egg = eggsAndFile.Item1
                           let file = eggsAndFile.Item2
                           select new { Guid = file.Guid, Path = egg.ComputeFullPath(file.InternalPath) };

         var downloadTasks = filesToPull.Select(file => cache.OpenOrAddAndOpenAsync(file.Guid, add => IoUtilities.ReadBytesAsync(file.Path)));
         foreach (var task in downloadTasks) {
            await task;
         }

         // clear stage
         if (Directory.Exists(stageDirectory)) {
            Directory.Delete(stageDirectory, true);
         }
         Directory.CreateDirectory(stageDirectory);

         // prepare stage
         var bundlesToDeleteByLowerName = (await localDeployment.EnumerateBundlesAsync()).ToDictionary(x => x.Name);
         foreach (var remoteBundle in await remoteDeployment.EnumerateBundlesAsync()) {
            var remoteBundleNameLower = remoteBundle.Name.ToLower();
            ManageableBundle localBundle;
            if (bundlesToDeleteByLowerName.TryGetValue(remoteBundleNameLower, out localBundle)) {
               bundlesToDeleteByLowerName.Remove(remoteBundleNameLower);
            }
            if (localBundle == null || localBundle.Version != remoteBundle.Version) {
               await BundleOperations.StageUpdate_UnderLock_Async(stageDirectory, remoteBundle);
            }
         }

         File.WriteAllText(IoUtilities.CombinePath(stageDirectory, NestConstants.kChannelFileName), remoteDeployment.Channel);
         File.WriteAllText(IoUtilities.CombinePath(stageDirectory, NestConstants.kRemoteFileName), remoteDeployment.Remote);
         File.WriteAllText(IoUtilities.CombinePath(stageDirectory, NestConstants.kVersionFileName), remoteDeployment.Version);

         var deleteString = string.Join("\r\n", bundlesToDeleteByLowerName.Values.Select(x => x.Name));
         File.WriteAllText(IoUtilities.CombinePath(stageDirectory, NestConstants.kDeleteFileName), deleteString);
         File.WriteAllText(IoUtilities.CombinePath(stageDirectory, NestConstants.kReadyFileName), "");
      }

      public void CommitUpdates(bool dryRun = false) {
         if (!DeploymentOperations.IsUpdateStaged(localDeployment)) return;

         var readyFilePath = IoUtilities.CombinePath(stageDirectory, NestConstants.kReadyFileName);
         using (File.OpenWrite(readyFilePath)) {
            // delete directories specified by delete string
            var deleteFilePath = IoUtilities.CombinePath(stageDirectory, NestConstants.kDeleteFileName);
            var bundlesToDelete = File.ReadAllLines(deleteFilePath);
            foreach (var bundleName in bundlesToDelete) {
               BundleOperations.CommitUpdate_DeleteBundle_UnderLock(localDeployment, bundleName, dryRun);
            }
            if (!dryRun) {
               File.Delete(deleteFilePath);
            }

            // move new / updated bundles
            foreach (var stagedBundlePath in Directory.EnumerateDirectories(stageDirectory)) {
               var stagedBundle = BundleFactory.Local(stagedBundlePath);
               BundleOperations.CommitUpdate_MoveBundle_UnderLock(localDeployment, stagedBundle, dryRun);
            }

            var rootFileNames = new[] { NestConstants.kChannelFileName, NestConstants.kRemoteFileName, NestConstants.kVersionFileName };
            foreach (var fileName in rootFileNames) {
               var stagedFilePath = IoUtilities.CombinePath(stageDirectory, fileName);
               var committedFilePath = IoUtilities.CombinePath(localDeployment.Location, fileName);
               if (File.Exists(stagedFilePath)) {
                  if (!dryRun) {
                     File.Copy(stagedFilePath, committedFilePath, true);
                     File.Delete(stagedFilePath);
                  }
               }
            }
         }
         if (!dryRun) {
            File.Delete(readyFilePath);
         }
      }

      public
         void EndUpdate() {
         deploymentLock?.Dispose();
         deploymentLock = null;
      }

      public void Dispose() {
         deploymentLock?.Dispose();
      }

      public IEnumerable<string> EnumerateStagedBundleNames() {
         foreach (var directory in new DirectoryInfo(stageDirectory).EnumerateDirectories()) {
            yield return directory.Name;
         }
      }
   }

   public static class DeploymentOperations {
      public static UpdateAvailability GetUpdateAvailability(ReadableDeployment localDeployment, ReadableDeployment remoteDeployment) {
         var localVersionString = localDeployment.Version;
         if (string.IsNullOrWhiteSpace(localVersionString)) {
            return UpdateAvailability.Major;
         }

         var localVersion = SemVersion.Parse(localVersionString);
         var remoteVersion = SemVersion.Parse(remoteDeployment.Version);
         if (localVersion.Major < remoteVersion.Major) {
            return UpdateAvailability.Major;
         } else if (localVersion.Minor < remoteVersion.Minor) {
            return UpdateAvailability.Minor;
         } else if (localVersion.Patch < remoteVersion.Patch) {
            return UpdateAvailability.Patch;
         } else {
            return UpdateAvailability.None;
         }
      }

      public static Task InstallAsync(string destinationDeploymentDirectory, ReadableDeployment remoteDeployment) {
         IoUtilities.PrepareDirectory(destinationDeploymentDirectory);

         // initialize to empty deployment
         File.WriteAllText(IoUtilities.CombinePath(destinationDeploymentDirectory, NestConstants.kChannelFileName), "");
         File.WriteAllText(IoUtilities.CombinePath(destinationDeploymentDirectory, NestConstants.kRemoteFileName), "");
         File.WriteAllText(IoUtilities.CombinePath(destinationDeploymentDirectory, NestConstants.kVersionFileName), "");

         return UpdateAsync(destinationDeploymentDirectory, remoteDeployment);
      }

      public static async Task UpdateAsync(string destinationDeploymentDirectory, ReadableDeployment remoteDeployment) {
         var localDeployment = DeploymentFactory.Local(destinationDeploymentDirectory);
         using (var updateContext = new DeploymentUpdateContext(localDeployment)) {
            try {
               updateContext.BeginUpdate();
               updateContext.SetRemoteDeployment(remoteDeployment);
               await updateContext.StageUpdatesAsync();
               updateContext.CommitUpdates();
            } finally {
               updateContext.EndUpdate();
            }
         }
      }

      public static void CommitUpdateIfStaged(ManageableDeployment localDeployment) {
         using (var updateContext = new DeploymentUpdateContext(localDeployment)) {
            try {
               updateContext.BeginUpdate();
               if (IsUpdateStaged(localDeployment)) {
                  updateContext.CommitUpdates();
               }
            } finally {
               updateContext.EndUpdate();
            }
         }
      }

      public static bool IsUpdateStaged(ReadableDeployment localDeployment) {
         var stageDirectory = IoUtilities.CombinePath(localDeployment.Location, NestConstants.kStageDirectoryName);
         var readyFilePath = IoUtilities.CombinePath(stageDirectory, NestConstants.kReadyFileName);
         return File.Exists(readyFilePath);
      }
   }
}