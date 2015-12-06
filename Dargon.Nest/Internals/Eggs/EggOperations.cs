using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Internals.Eggs.Common;

namespace Dargon.Nest.Internals.Eggs {
   public static class EggOperations {
      public static void Install(string destinationEggDirectory, ReadableEgg remoteEgg) {
         IoUtilities.PrepareDirectory(destinationEggDirectory);

         // initialize to empty nest
         File.WriteAllText(IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kFileListFileName), "");
         File.WriteAllText(IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kRemoteFileName), "");
         File.WriteAllText(IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kVersionFileName), "");

         Update(destinationEggDirectory, remoteEgg);
      }

      public static void Update(string destinationEggDirectory, ReadableEggRepository remoteRepository) {
         var remoteEgg = new ReadableEggProxy(new RepositoryBackedEggMetadata(remoteRepository), remoteRepository);
         Update(destinationEggDirectory, remoteEgg);
      }

      public static void Update(string destinationEggDirectory, ReadableEgg remoteEgg) {
         var bundlesDirectory = IoUtilities.GetAncestorInfoOfName(destinationEggDirectory, NestConstants.kBundlesDirectoryName);
         var cacheDirectory = Path.Combine(bundlesDirectory.FullName, "..", NestConstants.kCacheDirectoryName);
         var cache = new NestFileCache(cacheDirectory);

         var cacheFileStreamsByGuid = new Dictionary<Guid, FileStream>();

         try {
            // pull all remote files to cache and get reader stream
            foreach (var file in remoteEgg.EnumerateFiles()) {
               if (cacheFileStreamsByGuid.ContainsKey(file.Guid)) continue;
               var remotePath = remoteEgg.ComputeFullPath(file.InternalPath);
               var fileStream = cache.OpenOrAddAndOpen(file.Guid, add => IoUtilities.ReadBytes(remotePath));
               cacheFileStreamsByGuid.Add(file.Guid, fileStream);
            }

            // open all files in the egg directory for updating
            var existingFileStreamsByInternalPathLower = new Dictionary<string, FileStream>();
            foreach (var filePath in Directory.EnumerateFiles(destinationEggDirectory, "*", SearchOption.AllDirectories)) {
               var internalPathLower = IoUtilities.GetDescendentRelativePath(destinationEggDirectory, filePath).ToLower();
               var fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
               existingFileStreamsByInternalPathLower.Add(internalPathLower, fileStream);
            }

            // update / add to all files from remote egg
            foreach (var file in remoteEgg.EnumerateFiles()) {
               FileStream destinationFileStream = null;
               try {
                  var internalPathLower = file.InternalPath.ToLower();
                  if (existingFileStreamsByInternalPathLower.TryGetValue(internalPathLower, out destinationFileStream)) {
                     existingFileStreamsByInternalPathLower.Remove(internalPathLower);
                  } else {
                     var absolutePath = Path.Combine(destinationEggDirectory, file.InternalPath);
                     IoUtilities.PrepareParentDirectory(absolutePath);
                     destinationFileStream = File.Open(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                  }

                  var cacheFileStream = cacheFileStreamsByGuid[file.Guid];
                  cacheFileStream.Seek(0, SeekOrigin.Begin);
                  destinationFileStream.Seek(0, SeekOrigin.Begin);
                  cacheFileStream.CopyTo(destinationFileStream);
                  destinationFileStream.SetLength(cacheFileStream.Length);
               } finally {
                  destinationFileStream?.Dispose();
               }
            }

            // delete all remaining open file streams, as they don't exist in the updated egg.
            foreach (var kvp in existingFileStreamsByInternalPathLower) {
               var path = kvp.Value.Name;
               kvp.Value.Dispose();
               File.Delete(path);
            }

            // write file list, version info, remote
            var fileListString = EggFileEntrySerializer.Serialize(remoteEgg.EnumerateFiles());
            File.WriteAllText(Path.Combine(destinationEggDirectory, NestConstants.kFileListFileName), fileListString);
            File.WriteAllText(Path.Combine(destinationEggDirectory, NestConstants.kRemoteFileName), remoteEgg.Location);
            File.WriteAllText(Path.Combine(destinationEggDirectory, NestConstants.kVersionFileName), remoteEgg.Version);
         } finally {
            // free all reader streams
            foreach (var fs in cacheFileStreamsByGuid.Values) {
               fs.Dispose();
            }
         }
      }
   }
}
