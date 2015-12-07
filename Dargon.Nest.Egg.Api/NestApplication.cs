namespace Dargon.Nest.Eggs {
   public interface NestApplication {
      NestResult Start(HatchlingParameters parameters);
      NestResult Shutdown(ShutdownReason reason);
   }
}
