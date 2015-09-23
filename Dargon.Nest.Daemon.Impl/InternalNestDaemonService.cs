using System.Runtime.InteropServices;
using System.Threading;
using Dargon.Nest.Eggxecutor;

namespace Dargon.Nest.Daemon {
   [Guid("A06857FF-1D1F-491C-BF62-9EF3FE267C5F")]
   public interface InternalNestDaemonService {
      void KillDaemon();
      void WaitForShutdown();
   }

   public class InternalNestDaemonServiceImpl : InternalNestDaemonService {
      private readonly ManualResetEvent shutdownLatch = new ManualResetEvent(false);

      public void KillDaemon() {
         shutdownLatch.Set();
      }

      public void WaitForShutdown() {
         shutdownLatch.WaitOne();
      }
   }
}
