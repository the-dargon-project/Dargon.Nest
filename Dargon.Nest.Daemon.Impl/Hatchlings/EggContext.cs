using Fody.Constructors;

namespace Dargon.Nest.Daemon.Hatchlings {
   public class EggContext {
      private readonly ManageableEgg egg;
      private readonly BundleContext bundleContext;

      public EggContext(ManageableEgg egg, BundleContext bundleContext) {
         this.egg = egg;
         this.bundleContext = bundleContext;
      }

      public ManageableEgg Egg => egg;
      public string Name => egg.Name;
      public string Location => egg.Location;
      public BundleContext BundleContext => bundleContext;
   }
}