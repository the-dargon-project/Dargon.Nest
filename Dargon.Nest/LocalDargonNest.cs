using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace Dargon.Nest {
   public class LocalDargonNest : IDargonNest {
      private const string kRemoteSettingName = "REMOTE";
      private const string kChannelSettingName = "CHANNEL";
      private readonly string nestPath;

      public LocalDargonNest(string nestPath) {
         this.nestPath = nestPath;
      }

      public string Channel { get { return GetSetting(kChannelSettingName); } set { SetSetting(kChannelSettingName, value); } }
      public string Remote { get { return GetSetting(kRemoteSettingName); } set { SetSetting(kRemoteSettingName, value); } }

      public void UpdateNest() {
         var remote = Remote;
         var wc = new WebClient();
         var latestPackageRedirectorUrl = NestUtil.CombineUrl(remote, Channel);
         var latestPackageRelativeUrl = wc.DownloadString(latestPackageRedirectorUrl);
         var latestPackageUrl = NestUtil.CombineUrl(latestPackageRedirectorUrl, latestPackageRelativeUrl);
         var packageListUrl = NestUtil.CombineUrl(latestPackageUrl, "PACKAGES");
         var packageList = wc.DownloadString(packageListUrl);
         var eggsAndVersions = from line in packageList.Split('\n').Select(x => x.Trim())
                               let firstSpaceIndex = line.IndexOf(' ')
                               let eggName = line.Substring(0, firstSpaceIndex)
                               let version = line.Substring(firstSpaceIndex + 1)
                               select new { EggName = eggName, Version = version };
         foreach (var eggAndVersion in eggsAndVersions) {
            var name = eggAndVersion.EggName;
            var version = eggAndVersion.Version;
            var remoteEgg = RemoteDargonEgg.FromUrl($"{remote}/{name}-{version}", $"{remote}/{name}");
            if (!IsExistsEgg(name)) {
               InstallEgg(remoteEgg);
            } else {
               UpdateEggHelper(new LocalDargonEgg(GetEggPath(name)), remoteEgg);
            }
         }
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
            NestUtil.PrepareDirectory(localEggPath);
            localEggPath = new FileInfo(localEggPath).FullName;

            // copy file-list files
            var fileList = egg.Files;
            foreach (var file in fileList) {
               Console.WriteLine("Adding file " + file.InternalPath);
               var filePath = Path.Combine(localEggPath, file.InternalPath);
               NestUtil.PrepareParentDirectory(filePath);

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

      public bool IsExistsEgg(string eggName) => Directory.Exists(GetEggPath(eggName));
      private string GetEggPath(string eggName) => Path.Combine(nestPath, eggName);

      public void UpdateEgg(string eggName) {
         using (var nestLock = LocalNestLock.TakeLock(nestPath)) {
            var localEggPath = Path.Combine(nestPath, eggName);
            if (!Directory.Exists(localEggPath)) {
               throw new InvalidOperationException("Could not find egg `" + eggName + "`.");
            }

            var egg = new LocalDargonEgg(localEggPath);
            var remote = egg.Remote;

            if (string.IsNullOrWhiteSpace(remote)) {
               throw new InvalidOperationException("No remote defined for egg `" + eggName + "`.");
            }

            var remoteEgg = EggFactory.CreateEggFromRemote(remote);

            UpdateEggHelper(egg, remoteEgg);
         }
      }

      private void UpdateEggHelper(LocalDargonEgg localEgg, IDargonEgg remoteEgg) {
         var oldVersion = localEgg.Version;
         var oldFiles = localEgg.Files;
         var oldFilesByPath = oldFiles.ToDictionary(x => x.InternalPath);

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
            var localFilePath = Path.Combine(localEgg.RootPath, internalPath);
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
         File.WriteAllText(Path.Combine(localEgg.RootPath, NestConstants.kVersionFileName), remoteVersion);
         File.WriteAllText(Path.Combine(localEgg.RootPath, NestConstants.kFileListFileName), EggFileListSerializer.Serialize(remoteFiles));

         Console.WriteLine("Installed egg " + localEgg.Name + " version " + localEgg.Version + " to " + localEgg.RootPath);
      }

      public int ExecuteEgg(string eggName, string args) {
         var localEggPath = Path.Combine(nestPath, eggName);
         if (!Directory.Exists(localEggPath)) {
            throw new InvalidOperationException("Could not find egg `" + eggName + "`.");
         }

         var egg = new LocalDargonEgg(localEggPath);
         var executables = egg.Files.Where(f => f.InternalPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToArray();
         if (executables.Length == 0) {
            throw new InvalidOperationException("No executables found for egg " + eggName);
         } else if (executables.Length >= 2) {
            throw new InvalidOperationException("Ambiguous execute invocation. More than one exe found in directory");
         }

         Process.Start(Path.Combine(localEggPath, executables[0].InternalPath), args);
         return 0;
      }

      private string GetSetting(string settingName) {
         return File.ReadAllText(Path.Combine(nestPath, settingName), Encoding.UTF8);
      }

      private void SetSetting(string settingName, string value) {
         File.WriteAllText(Path.Combine(nestPath, settingName), value, Encoding.UTF8);
      }
   }
}