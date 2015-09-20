using Dargon.Nest.Eggxecutor;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface HatchlingContext {
      void Kill();
      NestContext Nest { get; }
      SpawnHatchlingResult SpawnResult { get; }
   }
}