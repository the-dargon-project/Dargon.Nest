namespace Dargon.Nest.Daemon.Hatchlings {
   public class EggContext {
      private readonly LocalDargonEgg egg;
      private readonly NestContext nestContext;

      public EggContext(LocalDargonEgg egg, NestContext nestContext) {
         this.egg = egg;
         this.nestContext = nestContext;
      }

      public LocalDargonEgg Egg => egg;
      public string Name => egg.Name;
      public string RootPath => egg.RootPath;
      public NestContext NestContext => nestContext;
   }
}