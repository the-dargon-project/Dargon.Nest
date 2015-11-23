using Fody.Constructors;
using ItzWarty.IO;
using System;
using System.IO;
using System.Threading;

namespace Dargon.Nest.Daemon.Hatchlings {
   [RequiredFieldsConstructor]
   public class NestLockManager {
      private const string kLockFileName = "LOCK";

      private readonly IFileSystemProxy fileSystemProxy = null;
      private readonly DaemonConfiguration daemonConfiguration = null;

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
