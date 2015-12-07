using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Bundles {
   public static class BundleOperations {
      public static async Task StageUpdate_UnderLock_Async(string stageDirectory, ReadableBundle remoteBundle) {
         var stagedBundleDirectory = IoUtilities.CombinePath(stageDirectory, remoteBundle.Name);
         IoUtilities.PrepareDirectory(stagedBundleDirectory);

         var stagedBundle = BundleFactory.Local(stagedBundleDirectory);
         var eggs = await remoteBundle.EnumerateEggsAsync();
         var installOperations = eggs.Select(stagedBundle.InstallEggAsync);
         foreach (var task in installOperations) {
            await task;
         }

         File.WriteAllText(IoUtilities.CombinePath(stagedBundleDirectory, NestConstants.kVersionFileName), remoteBundle.Version);
         File.WriteAllText(IoUtilities.CombinePath(stagedBundleDirectory, NestConstants.kRemoteFileName), remoteBundle.Remote);
         File.WriteAllText(IoUtilities.CombinePath(stagedBundleDirectory, NestConstants.kInitJsonFileName), remoteBundle.InitScript);
      }

      public static void CommitUpdate_DeleteBundle_UnderLock(ManageableDeployment localDeployment, string bundleName, bool dryRun = false) {
         var bundlePath = IoUtilities.CombinePath(localDeployment.Location, NestConstants.kBundlesDirectoryName, bundleName);
         if (dryRun) {
            foreach (var filePath in Directory.EnumerateFiles(bundlePath, "*", SearchOption.AllDirectories)) {
               using (var fs = File.OpenWrite(filePath)) {
                  // do nothing.
               }
            }
         } else {
            Directory.Delete(bundlePath, true);
         }
      }

      public static void CommitUpdate_MoveBundle_UnderLock(ManageableDeployment localDeployment, ManageableBundle stagedBundle, bool dryRun = false) {
         var destinationBundlePath = IoUtilities.CombinePath(localDeployment.Location, NestConstants.kBundlesDirectoryName, stagedBundle.Name);
         if (dryRun) {
            foreach (var filePath in Directory.EnumerateFiles(destinationBundlePath, "*", SearchOption.AllDirectories)) {
               using (var fs = File.OpenWrite(filePath)) {
                  // do nothing.
               }
            }
         } else {
            if (Directory.Exists(destinationBundlePath)) {
               Directory.Delete(destinationBundlePath, true);
            }
            Directory.Move(stagedBundle.Location, destinationBundlePath);
         }
      }
   }
}
