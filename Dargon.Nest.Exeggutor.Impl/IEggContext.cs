using Dargon.Nest.Eggxecutor;

namespace Dargon.Nest.Exeggutor {
   public interface IEggContext {
      IDargonEgg Egg { get; }

      HatchlingContext Spawn(SpawnConfiguration configuration);
   }
}