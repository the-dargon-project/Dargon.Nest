namespace Dargon.Nest.Egg {
   public interface IEggHost {
      void Shutdown();
      void Shutdown(ShutdownReason reason);
   }
}