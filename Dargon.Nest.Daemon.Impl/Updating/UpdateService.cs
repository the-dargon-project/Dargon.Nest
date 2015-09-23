using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Egg;
using ItzWarty;

namespace Dargon.Nest.Daemon.Updating {
   public class UpdateService {
      private readonly ReadableHatchlingDirectory hatchlingDirectory;
      private readonly StageManager stageManager;

      public UpdateService(ReadableHatchlingDirectory hatchlingDirectory, StageManager stageManager) {
         this.hatchlingDirectory = hatchlingDirectory;
         this.stageManager = stageManager;
      }

      public void ProcessStagedUpdate(string nestName) {
         if (stageManager.IsUpdateStaged(nestName)) {
            var hatchlings = hatchlingDirectory.EnumerateHatchlingsOfNest(nestName);
            hatchlings.ForEach(h => h.Shutdown(ShutdownReason.Update));
            stageManager.ProcessStagedUpdate(nestName);
         }
      }
   }
}
