using System.Runtime.InteropServices;
using Dargon.Nest.Eggxecutor;

namespace Dargon.Nest.Daemon {
   [Guid("A06857FF-1D1F-491C-BF62-9EF3FE267C5F")]
   public interface NestDaemonService : ExeggutorService {
      void KillHatchlingsAndDaemon();
   }
}
