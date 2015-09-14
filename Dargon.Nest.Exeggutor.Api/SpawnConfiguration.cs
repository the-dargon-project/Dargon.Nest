using Dargon.PortableObjects;

namespace Dargon.Nest.Eggxecutor {
   public class SpawnConfiguration : IPortableObject {
      public string InstanceName { get; set; }
      public byte[] Arguments { get; set; }
      public HatchlingStartFlags StartFlags { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteObject(0, InstanceName);
         writer.WriteBytes(1, Arguments);
         writer.WriteU32(2, (uint)StartFlags);
      }

      public void Deserialize(IPofReader reader) {
         InstanceName = reader.ReadObject<string>(0);
         Arguments = reader.ReadBytes(1);
         StartFlags = (HatchlingStartFlags)reader.ReadU32(2);
      }
   }
}
