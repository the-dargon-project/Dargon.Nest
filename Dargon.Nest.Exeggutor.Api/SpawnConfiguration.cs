using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Nest.Eggxecutor {
   public class SpawnConfiguration : IPortableObject {
      public string InstanceName { get; set; }
      public byte[] Arguments { get; set; }
      public HatchlingStartFlags StartFlags { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, InstanceName);
         writer.WriteBytes(1, Arguments);
         writer.WriteU32(2, (uint)StartFlags);
      }

      public void Deserialize(IPofReader reader) {
         InstanceName = reader.ReadString(0);
         Arguments = reader.ReadBytes(1);
         StartFlags = (HatchlingStartFlags)reader.ReadU32(2);
      }
   }
}
