using System.Threading;
using Dargon.Nest.Exeggutor;

namespace Dargon.Nest.Daemon {
   public class NestDaemonServiceImpl : NestDaemonService {
      private readonly ExeggutorServiceImpl exeggutorService;
      private readonly CountdownEvent shutdownLatch = new CountdownEvent(1);

      public NestDaemonServiceImpl(ExeggutorServiceImpl exeggutorService) {
         this.exeggutorService = exeggutorService;
         
      }

      public void Run() => shutdownLatch.Wait();

      public void KillHatchlingsAndNest() {
         exeggutorService.KillAllHatchlingsAndPrepareForShutdown();
         shutdownLatch.Signal();
      }
   }
}
