using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using System;

namespace Dargon.Nest.Eggxecutor {
   public class SpawnHatchlingResult : IPortableObject {
      public Guid HatchlingId { get; set; }
      public NestResult SpawnResult { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteGuid(0, HatchlingId);
         writer.WriteS32(1, (int)SpawnResult);
      }

      public void Deserialize(IPofReader reader) {
         HatchlingId = reader.ReadGuid(0);
         SpawnResult = (NestResult)reader.ReadS32(1);
      }
   }
}
