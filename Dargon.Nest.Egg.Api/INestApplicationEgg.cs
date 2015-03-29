namespace Dargon.Nest.Egg {
   public interface INestApplicationEgg {
      NestResult Start(object args);
      NestResult Shutdown();
   }

   public enum NestResult {
      Success,
      Failure
   }
}
