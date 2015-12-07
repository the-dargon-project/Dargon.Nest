using Dargon.Nest.Eggs;
using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class ShutdownDto : IPortableObject {
      public ShutdownReason Reason { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, (int)Reason);
      }

      public void Deserialize(IPofReader reader) {
         Reason = (ShutdownReason)reader.ReadS32(0);
      }
   }
}
