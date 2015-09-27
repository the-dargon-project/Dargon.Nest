using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Hatchlings {
   public class NestLockManager {
      private const string kLockFileName = "LOCK";

      private readonly IFileSystemProxy fileSystemProxy;
      private readonly DaemonConfiguration daemonConfiguration;

      public NestLockManager(IFileSystemProxy fileSystemProxy, DaemonConfiguration daemonConfiguration) {
         this.fileSystemProxy = fileSystemProxy;
         this.daemonConfiguration = daemonConfiguration;
      }

      public IDisposable TakeRead(string nestName) => Take(nestName, true);
      public IDisposable TakeWrite(string nestName) => Take(nestName, false);

      private IDisposable Take(string nestName, bool isRead) {
         var lockFilePath = Path.Combine(daemonConfiguration.NestsPath, nestName, kLockFileName);
         FileMode fileMode = FileMode.OpenOrCreate;
         FileAccess fileAccess = isRead ? FileAccess.Read : FileAccess.Write;
         FileShare fileShare = isRead ? FileShare.Read : FileShare.None;
         var spinner = new SpinWait();
         while (true) {
            try {
               return fileSystemProxy.OpenFile(lockFilePath, fileMode, fileAccess, fileShare);
            } catch (IOException) {
               spinner.SpinOnce();
            }
         }
      }
   }
}
