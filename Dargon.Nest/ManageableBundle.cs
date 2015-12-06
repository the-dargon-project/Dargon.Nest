using Dargon.Nest.Internals.Bundles;

namespace Dargon.Nest {
   public interface ManageableBundle : ReadableBundle, ManageableEggContainer {
      new ManageableEggContainer EggContainer { get; }
   }
}