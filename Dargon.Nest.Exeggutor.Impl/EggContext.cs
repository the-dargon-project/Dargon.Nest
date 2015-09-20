using Dargon.Nest.Eggxecutor;
using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public class EggContext : IEggContext {
      private readonly HatchlingContextInitializer hatchlingContextInitializer;
      private readonly IProcessProxy processProxy;
      private readonly IDargonEgg egg;

      public EggContext(HatchlingContextInitializer hatchlingContextInitializer, IProcessProxy processProxy, IDargonEgg egg) {
         this.hatchlingContextInitializer = hatchlingContextInitializer;
         this.processProxy = processProxy;
         this.egg = egg;
      }

      public IDargonEgg Egg => egg;

      public HatchlingContext Spawn(SpawnConfiguration configuration) {
         var context = hatchlingContextInitializer.Spawn(configuration.InstanceName, egg);
         context.StartBootstrap(configuration.Arguments);
         return context;
      }
   }
}