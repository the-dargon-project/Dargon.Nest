using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Eggs;
using Dargon.PortableObjects;

namespace Dargon.Nest.Exeggutor.Host.PortableObjects {
   public class BootstrapResultDto : IPortableObject {
      public BootstrapResultDto() { }

      public BootstrapResultDto(NestResult startResult) {
         this.StartResult = startResult;
      }

      public NestResult StartResult { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, (int)StartResult);
      }

      public void Deserialize(IPofReader reader) {
         StartResult = (NestResult)reader.ReadS32(0);
      }
   }
}
