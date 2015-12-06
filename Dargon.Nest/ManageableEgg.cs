using Dargon.Nest.Internals.Eggs;

namespace Dargon.Nest {
   public interface ManageableEgg : ReadableEgg, ManageableEggRepository {
      new ManageableEggRepository Repository { get; }
   }
}