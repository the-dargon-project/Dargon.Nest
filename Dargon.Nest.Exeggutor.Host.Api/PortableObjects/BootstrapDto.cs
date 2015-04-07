using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class BootstrapDto : IPortableObject {
      private string name;
      private string eggPath;
      private byte[] payloadBytes;

      public BootstrapDto() { }

      public BootstrapDto(string name, string eggPath, byte[] payloadBytes) {
         this.name = name;
         this.eggPath = eggPath;
         this.payloadBytes = payloadBytes;
      }

      public string Name { get { return name; } set { name = value; } }
      public string EggPath { get { return eggPath; } set { eggPath = value; } }
      public byte[] PayloadBytes { get { return payloadBytes; } set { payloadBytes = value; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, name);
         writer.WriteString(1, eggPath);
         writer.WriteCollection(2, payloadBytes);
      }

      public void Deserialize(IPofReader reader) {
         name = reader.ReadString(0);
         eggPath = reader.ReadString(1);
         payloadBytes = reader.ReadArray<byte>(2);
      }
   }
}
