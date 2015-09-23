using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Hatchlings {
   public class NestDirectorySynchronizer {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly DaemonConfiguration daemonConfiguration;
      private readonly NestDirectory nestDirectory;
      private readonly NestContextFactory nestContextFactory;

      public NestDirectorySynchronizer(IFileSystemProxy fileSystemProxy, DaemonConfiguration daemonConfiguration, NestDirectory nestDirectory, NestContextFactory nestContextFactory) {
         this.fileSystemProxy = fileSystemProxy;
         this.daemonConfiguration = daemonConfiguration;
         this.nestDirectory = nestDirectory;
         this.nestContextFactory = nestContextFactory;
      }

      public void Initialize() {
         var watcher = new FileSystemWatcher(daemonConfiguration.NestsPath);
         watcher.Created += HandleCreated;
         watcher.Deleted += HandleDeleted;
         watcher.Renamed += HandleRenamed;
         watcher.EnableRaisingEvents = true;

         foreach (var existingNestDirectory in fileSystemProxy.GetDirectoryInfo(daemonConfiguration.NestsPath).EnumerateDirectories()) {
            HandleCreated(watcher, new FileSystemEventArgs(WatcherChangeTypes.Created, existingNestDirectory.Parent.FullName, existingNestDirectory.Name));
         }
      }

      private void HandleCreated(object sender, FileSystemEventArgs e) {
         var fileInfo = fileSystemProxy.GetFileInfo(e.FullPath);
         if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
            return;
         }
         var nestContext = nestContextFactory.Create(e.FullPath);
         nestDirectory.Register(nestContext);
      }

      private void HandleDeleted(object sender, FileSystemEventArgs e) {
         var fileInfo = fileSystemProxy.GetFileInfo(e.FullPath);
         if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
            return;
         }
         var nestContext = nestDirectory.GetNestContextByName(e.Name);
         nestDirectory.Unregister(nestContext);
      }

      private void HandleRenamed(object sender, RenamedEventArgs e) {
         HandleDeleted(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, e.OldFullPath, e.OldName));
         HandleCreated(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, e.FullPath, e.Name));
      }
   }
}
