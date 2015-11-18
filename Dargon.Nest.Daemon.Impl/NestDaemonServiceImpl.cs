using System.Threading;

namespace Dargon.Nest.Daemon {
   public class NestDaemonServiceImpl : NestDaemonService {
      private readonly ManualResetEvent shutdownLatch = new ManualResetEvent(false);

      public void KillDaemon() {
         shutdownLatch.Set();
      }

      public void WaitForShutdown() {
         shutdownLatch.WaitOne();
      }
   }
}