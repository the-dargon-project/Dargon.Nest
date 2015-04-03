using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public interface EggContextFactory {
      IEggContext Create(IDargonEgg egg);
   }

   public class EggContextFactoryImpl : EggContextFactory {
      private readonly RemoteHostContextFactory remoteHostContextFactory;
      private readonly IProcessProxy processProxy;

      public EggContextFactoryImpl(RemoteHostContextFactory remoteHostContextFactory, IProcessProxy processProxy) {
         this.remoteHostContextFactory = remoteHostContextFactory;
         this.processProxy = processProxy;
      }

      public IEggContext Create(IDargonEgg egg) {
         return new EggContext(remoteHostContextFactory, processProxy, egg);
      }
   }
}