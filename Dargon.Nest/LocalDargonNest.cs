using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dargon.Nest {
   public class LocalDargonNest : IDargonNest {
      private readonly string nestPath;

      public LocalDargonNest(string nestPath) {
         this.nestPath = nestPath;
      }

      public IEnumerable<IDargonEgg> EnumerateEggs() {
         foreach (var directory in Directory.EnumerateDirectories(nestPath)) {
            LocalDargonEgg egg = null;
            try {
               egg = new LocalDargonEgg(directory);
            } catch (Exception e) {
               Console.WriteLine(e);
            }
            if (egg != null) {
               yield return egg;
            }
         }
      }

      public void InstallEgg(IDargonEgg egg) {
         using (var nestLock = LocalNestLock.TakeLock(nestPath)) {
            var localEggPath = Path.Combine(nestPath, egg.Name);
            Util.PrepareDirectory(localEggPath);
            localEggPath = new FileInfo(localEggPath).FullName;

            // copy file-list files
            var fileList = egg.Files;
            foreach (var file in fileList) {
               Console.WriteLine("Adding file " + file.InternalPath);
               var filePath = Path.Combine(localEggPath, file.InternalPath);
               Util.PrepareParentDirectory(filePath);

               using (var sourceStream = egg.GetStream(file.InternalPath))
               using (var destStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                  sourceStream.CopyTo(destStream);
               }
            }

            // create version and filelist files
            Console.WriteLine("Copy version and filelist files.");
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kVersionFileName), egg.Version);
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kFileListFileName), EggFileListSerializer.Serialize(fileList));

            Console.WriteLine("Setting remote to " + egg.Remote);
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kRemoteFileName), egg.Remote);

            Console.WriteLine("Installed egg " + egg.Name + " version " + egg.Version + " to " + localEggPath);
         }
      }

      public void UpdateEgg(string eggName) {
         using (var nestLock = LocalNestLock.TakeLock(nestPath)) {
            var localEggPath = Path.Combine(nestPath, eggName);
            if (!Directory.Exists(localEggPath)) {
               throw new InvalidOperationException("Could not find egg `" + eggName + "`.");
            }

            var egg = new LocalDargonEgg(localEggPath);
            var remote = egg.Remote;
            var oldVersion = egg.Version;
            var oldFiles = egg.Files;
            var oldFilesByPath = oldFiles.ToDictionary(x => x.InternalPath);

            if (string.IsNullOrWhiteSpace(remote)) {
               throw new InvalidOperationException("No remote defined for egg `" + eggName + "`.");
            }

            var remoteEgg = EggFactory.CreateEggFromRemote(remote);
            var remoteVersion = remoteEgg.Version;
            var remoteFiles = remoteEgg.Files;
            var remoteFilesByPath = remoteFiles.ToDictionary(x => x.InternalPath);

            if (oldVersion != remoteVersion) {
               Console.WriteLine("Update available: " + oldVersion + " => " + remoteVersion + ".");
            } else {
               Console.WriteLine("No update available. Already have version " + remoteVersion + ".");
               return;
            }

            var oldFilePaths = new HashSet<string>(oldFiles.Select(x => x.InternalPath));
            var remoteFilePaths = new HashSet<string>(remoteFiles.Select(x => x.InternalPath));
            var allFilePaths = new SortedSet<string>(oldFilePaths.Concat(remoteFilePaths));

            foreach (var internalPath in allFilePaths) {
               var oldExists = oldFilePaths.Contains(internalPath);
               var remoteExists = remoteFilePaths.Contains(internalPath);
               var localFilePath = Path.Combine(localEggPath, internalPath);
               if (oldExists && remoteExists) {
                  if (oldFilesByPath[internalPath].Guid != remoteFilesByPath[internalPath].Guid) {
                     Console.WriteLine("Updating file " + internalPath);
                     using (var sourceStream = remoteEgg.GetStream(internalPath))
                     using (var destStream = File.Open(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                        sourceStream.CopyTo(destStream);
                     }
                  } else {
                     Console.WriteLine("Keeping file " + internalPath);
                  }
               } else if (oldExists && !remoteExists) {
                  Console.WriteLine("Deleting file " + internalPath);
                  File.Delete(localFilePath);
               } else if (!oldExists && remoteExists) {
                  Console.WriteLine("Adding file " + internalPath);
                  using (var sourceStream = remoteEgg.GetStream(internalPath))
                  using (var destStream = File.Open(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                     sourceStream.CopyTo(destStream);
                  }
               }
            }

            Console.WriteLine("Update version and filelist files.");
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kVersionFileName), remoteVersion);
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kFileListFileName), EggFileListSerializer.Serialize(remoteFiles));

            Console.WriteLine("Installed egg " + egg.Name + " version " + egg.Version + " to " + localEggPath);
         }
      }
   }
}