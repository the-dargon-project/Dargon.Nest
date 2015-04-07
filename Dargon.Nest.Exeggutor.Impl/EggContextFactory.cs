using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public interface EggContextFactory {
      IEggContext Create(IDargonEgg egg);
   }

   public class EggContextFactoryImpl : EggContextFactory {
      private readonly HatchlingContextFactory hatchlingContextFactory;
      private readonly IProcessProxy processProxy;

      public EggContextFactoryImpl(HatchlingContextFactory hatchlingContextFactory, IProcessProxy processProxy) {
         this.hatchlingContextFactory = hatchlingContextFactory;
         this.processProxy = processProxy;
      }

      public IEggContext Create(IDargonEgg egg) {
         return new EggContext(hatchlingContextFactory, processProxy, egg);
      }
   }
}