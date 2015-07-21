using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class ShutdownDto : IPortableObject {
      public ShutdownDto() { }
      public void Serialize(IPofWriter writer) { }
      public void Deserialize(IPofReader reader) { }
   }
}
