using Dargon.Nest.Eggxecutor;
using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public class EggContext : IEggContext {
      private readonly HatchlingContextFactory hatchlingContextFactory;
      private readonly IProcessProxy processProxy;
      private readonly IDargonEgg egg;

      public EggContext(HatchlingContextFactory hatchlingContextFactory, IProcessProxy processProxy, IDargonEgg egg) {
         this.hatchlingContextFactory = hatchlingContextFactory;
         this.processProxy = processProxy;
         this.egg = egg;
      }

      public HatchlingContext Spawn(SpawnConfiguration configuration) {
         var context = hatchlingContextFactory.Create(configuration.InstanceName, egg.Location);
         context.Bootstrap(configuration.Arguments);
         return context;
      }
   }
}