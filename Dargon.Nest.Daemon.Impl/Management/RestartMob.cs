using Dargon.Management;
using Dargon.Nest.Daemon.Restart;
using Fody.Constructors;

namespace Dargon.Nest.Daemon.Management {
   [RequiredFieldsConstructor]
   public class RestartMob {
      private readonly RestartSignalService restartSignalService = null;

      [ManagedOperation]
      public void ProcessRestartSignals() => restartSignalService.ProcessRestartSignals();
   }
}
