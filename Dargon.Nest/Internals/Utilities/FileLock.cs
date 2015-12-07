using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Utilities {
   public static class FileLock {
      public static IDisposable Take(string path) {
         var spinner = new SpinWait();
         IDisposable disposable;
         while (!TryTake(path, out disposable)) {
            spinner.SpinOnce();
         }
         return disposable;
      }

      public static bool TryTake(string path,  out IDisposable lockDisposable) {
         try {
            lockDisposable = File.OpenWrite(path);
            return true;
         } catch (IOException) {
            lockDisposable = null;
            return false;
         }
      }
   }
}
