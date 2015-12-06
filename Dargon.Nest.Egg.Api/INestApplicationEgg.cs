namespace Dargon.Nest.Eggs {
   public interface INestApplicationEgg {
      NestResult Start(IEggParameters parameters);
      NestResult Shutdown(ShutdownReason reason);
   }
}
