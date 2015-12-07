using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Internals.Eggs.Local;
using Dargon.Nest.Internals.Eggs.Remote;

namespace Dargon.Nest.Internals.Eggs {
   public static class EggOperations {
      public static Task InstallAsync(string destinationEggDirectory, ReadableEgg remoteEgg) {
         IoUtilities.PrepareDirectory(destinationEggDirectory);

         // initialize to empty nest
         File.WriteAllText(IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kFileListFileName), "");
         File.WriteAllText(IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kRemoteFileName), "");
         File.WriteAllText(IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kVersionFileName), "");

         return UpdateAsync(destinationEggDirectory, remoteEgg);
      }

      public static async Task UpdateAsync(string destinationEggDirectory, ReadableEgg remoteEgg) {
         IoUtilities.PrepareDirectory(destinationEggDirectory);

         var fileListContents = "";
         var fileListPath = IoUtilities.CombinePath(destinationEggDirectory, NestConstants.kFileListFileName);
         if (File.Exists(fileListPath)) {
            fileListContents = File.ReadAllText(fileListPath);
         }
         var fileList = EggFileEntrySerializer.Deserialize(fileListContents);
         var existingHashesByInternalPathLower = fileList.ToDictionary(entry => entry.InternalPath.ToLower(), entry => entry.Guid);
         
         var cacheDirectory = IoUtilities.FindAncestorCachePath(destinationEggDirectory);
         var cache = new NestFileCache(cacheDirectory);
         var cacheFileStreamsByGuid = new Dictionary<Guid, FileStream>();

         try {
            var remoteEggFiles = (await remoteEgg.EnumerateFilesAsync()).ToArray();

            // pull all remote files to cache and get reader stream
            foreach (var file in remoteEggFiles) {
               if (cacheFileStreamsByGuid.ContainsKey(file.Guid)) continue;
               var remotePath = remoteEgg.ComputeFullPath(file.InternalPath);
               var fileStream = await cache.OpenOrAddAndOpenAsync(file.Guid, add => IoUtilities.ReadBytesAsync(remotePath));
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
            foreach (var file in remoteEggFiles) {
               FileStream destinationFileStream = null;
               try {
                  var internalPathLower = file.InternalPath.ToLower();
                  if (existingFileStreamsByInternalPathLower.TryGetValue(internalPathLower, out destinationFileStream)) {
                     existingFileStreamsByInternalPathLower.Remove(internalPathLower);

                     // don't overwrite if we already have this file version.
                     Guid existingHash;
                     if (existingHashesByInternalPathLower.TryGetValue(internalPathLower, out existingHash)) {
                        if (existingHash == file.Guid) {
                           continue;
                        }
                     }
                  } else {
                     var absolutePath = Path.Combine(destinationEggDirectory, file.InternalPath);
                     IoUtilities.PrepareParentDirectory(absolutePath);
                     destinationFileStream = File.Open(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                  }

                  var cacheFileStream = cacheFileStreamsByGuid[file.Guid];
                  cacheFileStream.Seek(0, SeekOrigin.Begin);
                  destinationFileStream.Seek(0, SeekOrigin.Begin);
                  await cacheFileStream.CopyToAsync(destinationFileStream);
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
            var fileListString = EggFileEntrySerializer.Serialize(remoteEggFiles);
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
