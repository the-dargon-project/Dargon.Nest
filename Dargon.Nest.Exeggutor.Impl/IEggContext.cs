using Dargon.Nest.Eggxecutor;

namespace Dargon.Nest.Exeggutor {
   public interface IEggContext {
      HatchlingContext Spawn(SpawnConfiguration configuration);
   }
}