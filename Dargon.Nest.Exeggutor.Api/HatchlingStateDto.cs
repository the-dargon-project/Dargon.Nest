using Dargon.Nest.Eggs;
using System;
using Dargon.PortableObjects;

namespace Dargon.Nest.Eggxecutor {
   public class HatchlingStateDto : IPortableObject {
      public Guid Id { get; set; }
      public string Name { get; set; }
      public NestResult StartResult { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteGuid(0, Id);
         writer.WriteString(1, Name);
         writer.WriteS32(2, (int)StartResult);
      }

      public void Deserialize(IPofReader reader) {
         Id = reader.ReadGuid(0);
         Name = reader.ReadString(1);
         StartResult = (NestResult)reader.ReadS32(2);
      }
   }
}
