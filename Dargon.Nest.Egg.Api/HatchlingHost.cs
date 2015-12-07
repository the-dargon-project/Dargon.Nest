namespace Dargon.Nest.Eggs {
   public interface HatchlingHost {
      void Shutdown();
      void Shutdown(ShutdownReason reason);

      void SetRestartToken(HatchlingParameters parameters);
   }
}