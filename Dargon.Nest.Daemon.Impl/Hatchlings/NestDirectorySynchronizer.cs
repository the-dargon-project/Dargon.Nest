using Fody.Constructors;
using ItzWarty.IO;
using System.IO;

namespace Dargon.Nest.Daemon.Hatchlings {
    [RequiredFieldsConstructor]
    public class NestDirectorySynchronizer {
      private readonly IFileSystemProxy fileSystemProxy = null;
      private readonly DaemonConfiguration daemonConfiguration = null;
      private readonly NestDirectoryImpl nestDirectory = null;
      private readonly NestContextFactory nestContextFactory = null;

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
