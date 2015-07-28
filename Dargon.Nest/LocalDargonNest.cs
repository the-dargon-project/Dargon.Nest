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

      public string CachePath => Path.Combine(nestPath, ".cache");
      public string Channel { get { return GetSetting(kChannelSettingName); } set { SetSetting(kChannelSettingName, value); } }
      public string Remote { get { return GetSetting(kRemoteSettingName); } set { SetSetting(kRemoteSettingName, value); } }

      public void UpdateNest() => UpdateNest(new UpdateNestOptions());

      public void UpdateNest(UpdateNestOptions updateNestOptions) {
         var excludedEggs = updateNestOptions.ExcludedEggs;
         var updateState = updateNestOptions.UpdateState;

         var remote = Remote;
         var wc = new WebClient();
         var latestPackageRedirectorUrl = NestUtil.CombineUrl(remote, Channel);
         var latestPackageRelativeUrl = wc.DownloadString(latestPackageRedirectorUrl);
         var latestPackageUrl = NestUtil.CombineUrl(latestPackageRedirectorUrl, latestPackageRelativeUrl);
         var packageListUrl = NestUtil.CombineUrl(latestPackageUrl, "PACKAGES");
         updateState.SetState($"Downloading Package List for Channel {Channel}.", 0);
         var packageList = wc.DownloadString(packageListUrl);
         var eggsAndVersions = (from line in packageList.Split('\n').Select(x => x.Trim())
                                let firstSpaceIndex = line.IndexOf(' ')
                                let eggName = line.Substring(0, firstSpaceIndex)
                                let version = line.Substring(firstSpaceIndex + 1)
                                select new { EggName = eggName, Version = version }).ToArray();
         for (var eggIndex = 0; eggIndex < eggsAndVersions.Length; eggIndex++) {
            var eggAndVersion = eggsAndVersions[eggIndex];
            var name = eggAndVersion.EggName;

            if (excludedEggs.Any(excludedEggName => excludedEggName.Equals(name, StringComparison.OrdinalIgnoreCase))) {
               continue;
            }

            var version = eggAndVersion.Version;
            var remoteEgg = RemoteDargonEgg.FromUrl($"{remote}/{name}-{version}", $"{remote}/{name}");

            updateState.SetState($"Updating Dargon Egg `{name}`!", (double)eggIndex / eggsAndVersions.Length);
            if (!IsExistsEgg(name)) {
               InstallEgg(remoteEgg, updateState);
            } else {
               UpdateEggHelper(new LocalDargonEgg(GetEggPath(name)), remoteEgg, updateState);
            }
         }
         updateState.SetState($"Update Complete!", 1);
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

      public void InstallEgg(IDargonEgg egg) => InstallEgg(egg, new UpdateState());

      public void InstallEgg(IDargonEgg egg, UpdateState updateState) {
         using (var nestLock = LocalNestLock.TakeLock(nestPath)) {
            var localEggPath = Path.Combine(nestPath, egg.Name);
            NestUtil.PrepareDirectory(localEggPath);
            localEggPath = new FileInfo(localEggPath).FullName;

            // copy file-list files
            var fileList = egg.Files;
            for (var fileIndex = 0; fileIndex < fileList.Count; fileIndex++) {
               var file = fileList[fileIndex];
               updateState.SetSubState("Adding file " + file.InternalPath, (double)fileIndex / fileList.Count);

               var filePath = Path.Combine(localEggPath, file.InternalPath);
               NestUtil.PrepareParentDirectory(filePath);

               var cacheFilePath = Path.Combine(CachePath, file.Guid.ToString("n"));
               var cacheFileExists = File.Exists(cacheFilePath);
               if (cacheFileExists) {
                  NestUtil.PrepareParentDirectory(filePath);
                  File.Copy(cacheFilePath, filePath, true);
               } else {
                  using (var destStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                  using (var sourceStream = egg.GetStream(file.InternalPath)) {
                     sourceStream.CopyTo(destStream);
                  }
                  Directory.CreateDirectory(CachePath);
                  File.SetAttributes(CachePath, File.GetAttributes(CachePath) | FileAttributes.Hidden);
                  File.Copy(filePath, cacheFilePath);
               }
            }

            // create version and filelist files
            updateState.SetSubState("Copy version and filelist files.", 1);
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kVersionFileName), egg.Version);
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kFileListFileName), EggFileListSerializer.Serialize(fileList));

            updateState.SetSubState("Setting remote to " + egg.Remote, 1);
            File.WriteAllText(Path.Combine(localEggPath, NestConstants.kRemoteFileName), egg.Remote);

            updateState.SetSubState("Installed egg " + egg.Name + " version!", 1);
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

            UpdateEggHelper(egg, remoteEgg, new UpdateState());
         }
      }

      private void UpdateEggHelper(LocalDargonEgg localEgg, IDargonEgg remoteEgg, UpdateState updateState) {
         var oldVersion = localEgg.Version;
         var oldFiles = localEgg.Files;
         var oldFilesByPath = oldFiles.ToDictionary(x => x.InternalPath);

         var remoteVersion = remoteEgg.Version;
         var remoteFiles = remoteEgg.Files;
         var remoteFilesByPath = remoteFiles.ToDictionary(x => x.InternalPath);

         if (oldVersion != remoteVersion) {
            updateState.Status = "Updating Dargon Egg `{localEgg.Name}` {oldVersion} => {remoteVersion}!";
         } else {
            updateState.SetSubState($"No update available. Already have version {remoteVersion}.", 0);
            return;
         }

         var oldFilePaths = new HashSet<string>(oldFiles.Select(x => x.InternalPath));
         var remoteFilePaths = new HashSet<string>(remoteFiles.Select(x => x.InternalPath));
         var allFilePaths = new SortedSet<string>(oldFilePaths.Concat(remoteFilePaths)).ToArray();

//         foreach (var internalPath in allFilePaths) {
         for (var fileIndex = 0; fileIndex < allFilePaths.Length; fileIndex++) {
            var internalPath = allFilePaths[fileIndex];
            var percentage = (double)fileIndex / allFilePaths.Length;

            var oldExists = oldFilePaths.Contains(internalPath);
            var remoteExists = remoteFilePaths.Contains(internalPath);
            var localFilePath = Path.Combine(localEgg.RootPath, internalPath);

            Action addOrUpdate = () => {
               var remoteFile = remoteEgg.Files.First(file => file.InternalPath.Equals(internalPath));

               var cacheFilePath = Path.Combine(CachePath, remoteFile.Guid.ToString("n"));
               var cacheFileExists = File.Exists(cacheFilePath);

               if (cacheFileExists) {
                  NestUtil.PrepareParentDirectory(localFilePath);
                  File.Copy(cacheFilePath, localFilePath, true);
               } else {
                  using (var destStream = File.Open(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                  using (var sourceStream = remoteEgg.GetStream(remoteFile.InternalPath)) {
                     sourceStream.CopyTo(destStream);
                  }
                  Directory.CreateDirectory(CachePath);
                  File.SetAttributes(CachePath, File.GetAttributes(CachePath) | FileAttributes.Hidden);
                  File.Copy(localFilePath, cacheFilePath);
               }
            };

            if (oldExists && remoteExists) {
               if (oldFilesByPath[internalPath].Guid != remoteFilesByPath[internalPath].Guid) {
                  updateState.SetSubState($"Updating file {internalPath}.", percentage);
                  addOrUpdate();
               } else {
                  updateState.SetSubState($"Keeping file {internalPath}.", percentage);
               }
            } else if (oldExists && !remoteExists) {
               updateState.SetSubState($"Deleting file {internalPath}.", percentage);
               File.Delete(localFilePath);
            } else if (!oldExists && remoteExists) {
               updateState.SetSubState($"Adding file {internalPath}.", percentage);
               addOrUpdate();
            }
         }

         updateState.SetSubState("Update version and filelist files.", 1);
         File.WriteAllText(Path.Combine(localEgg.RootPath, NestConstants.kVersionFileName), remoteVersion);
         File.WriteAllText(Path.Combine(localEgg.RootPath, NestConstants.kFileListFileName), EggFileListSerializer.Serialize(remoteFiles));

         updateState.SetSubState($"Installed egg `{localEgg.Name}` version {localEgg.Version}!", 1);
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