namespace Dargon.Nest.Eggs {
   public interface IEggHost {
      void Shutdown();
      void Shutdown(ShutdownReason reason);
   }
}