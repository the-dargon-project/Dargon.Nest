using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class ExeggutorHostPofContext : PofContext {
      public ExeggutorHostPofContext(int basePofId) {
         RegisterPortableObjectType(basePofId + 0, typeof(BootstrapDto));
         RegisterPortableObjectType(basePofId + 1, typeof(ShutdownDto));
      }
   }
}
