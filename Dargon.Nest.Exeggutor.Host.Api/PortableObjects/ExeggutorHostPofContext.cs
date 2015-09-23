using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class ExeggutorHostPofContext : PofContext {
      private const int kBasePofId = 3500;

      public ExeggutorHostPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(BootstrapDto));
         RegisterPortableObjectType(kBasePofId + 1, typeof(ShutdownDto));
         RegisterPortableObjectType(kBasePofId + 2, typeof(BootstrapResultDto));
      }
   }
}
