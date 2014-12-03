using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public class LocalNestLock : IDisposable {
      private static readonly object kApplicationNestLock = new object();
      private static readonly Dictionary<string, LocalNestLock> locksByNestPath = new Dictionary<string, LocalNestLock>();

      public static LocalNestLock TakeLock(string nestPath) {
         nestPath = new FileInfo(nestPath).FullName;
         lock (kApplicationNestLock) {
            LocalNestLock nestLock;
            if (locksByNestPath.TryGetValue(nestPath, out nestLock)) {
               nestLock.IncrementCounter();
            } else {
               nestLock = new LocalNestLock(nestPath);
               locksByNestPath.Add(nestPath, nestLock);
            }
            return nestLock;
         }
      }

      private static void HandleLockFreed(LocalNestLock localNestLock) {
         lock (kApplicationNestLock) {
            locksByNestPath.Remove(localNestLock.nestPath);
         }
      }

      private readonly string nestPath;
      private readonly string lockPath;
      private readonly FileStream file;
      private int count = 0;

      public LocalNestLock(string nestPath) {
         this.nestPath = nestPath;
         this.lockPath = Path.Combine(nestPath, "LOCK");
         this.file = File.Open(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

         IncrementCounter();
      }

      internal int IncrementCounter() {
         lock (kApplicationNestLock) {
            count++;
            return count;
         }
      }

      internal int DecrementCounter() {
         lock (kApplicationNestLock) {
            count--;
            return count;
         }
      }

      public void Dispose() {
         lock (kApplicationNestLock) {
            if (DecrementCounter() == 0) {
               file.Dispose();
               HandleLockFreed(this);
            }
         }
      }
   }
}
