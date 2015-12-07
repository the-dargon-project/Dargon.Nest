using System;
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

      public void RestartDaemon() {
         AppDomain.CurrentDomain.ProcessExit += RestartExitHandler;
         KillDaemon();
      }

      private void RestartExitHandler(object sender, EventArgs e) {
         InitUtilities.ExecInit();
      }
   }

   public static class InitUtilities {
      public static void ExecInit() {

      }
   }
}