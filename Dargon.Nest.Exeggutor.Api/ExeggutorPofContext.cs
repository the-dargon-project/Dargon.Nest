using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Nest.Eggxecutor {
   public class ExeggutorPofContext : PofContext {
      public ExeggutorPofContext(int basePofId) {
         RegisterPortableObjectType(basePofId + 0, typeof(SpawnConfiguration));
         RegisterPortableObjectType(basePofId + 1, typeof(SpawnHatchlingResult));
      }
   }
}
