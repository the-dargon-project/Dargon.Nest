namespace Dargon.Nest.Exeggutor {
   public interface EggContextFactory {
      IEggContext Create(IDargonEgg egg);
   }

   public class EggContextFactoryImpl : EggContextFactory {
      public IEggContext Create(IDargonEgg egg) {
         return new EggContext(egg);
      }
   }
}