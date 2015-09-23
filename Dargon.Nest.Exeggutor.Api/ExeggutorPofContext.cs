using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Nest.Eggxecutor {
   public class ExeggutorPofContext : PofContext {
      private const int kBasePofId = 3000;

      public ExeggutorPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(SpawnConfiguration));
         RegisterPortableObjectType(kBasePofId + 1, typeof(SpawnHatchlingResult));
      }
   }
}
