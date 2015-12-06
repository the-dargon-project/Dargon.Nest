using Dargon.Nest.Eggs;
using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class ShutdownDto : IPortableObject {
      public ShutdownReason Reason { get; set; }

      public void Serialize(IPofWriter writer) {

      }
      public void Deserialize(IPofReader reader) { }
   }
}
