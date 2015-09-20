using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public interface EggContextFactory {
      IEggContext Create(IDargonEgg egg);
   }

   public class EggContextFactoryImpl : EggContextFactory {
      private readonly HatchlingContextInitializer hatchlingContextInitializer;
      private readonly IProcessProxy processProxy;

      public EggContextFactoryImpl(HatchlingContextInitializer hatchlingContextInitializer, IProcessProxy processProxy) {
         this.hatchlingContextInitializer = hatchlingContextInitializer;
         this.processProxy = processProxy;
      }

      public IEggContext Create(IDargonEgg egg) {
         return new EggContext(hatchlingContextInitializer, processProxy, egg);
      }
   }
}