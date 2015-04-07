namespace Dargon.Nest.Egg {
   public interface INestApplicationEgg {
      NestResult Start(IEggParameters parameters);
      NestResult Shutdown();
   }

   public enum NestResult {
      Success,
      Failure
   }
}
