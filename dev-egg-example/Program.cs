using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dargon.Nest.Egg;
using Dargon.PortableObjects;

namespace dev_egg_example {
   public class ExampleEgg : INestApplicationEgg {
      public ExampleEgg() {

      }

      public NestResult Start(IEggParameters parameters) {
         var args = new PofSerializer(new PofContext()).Deserialize(new MemoryStream(parameters.Arguments));

         if (!(args is string)) {
            return NestResult.Failure;
         } else {
            return Start((string)args);
         }
      }

      private NestResult Start(string args) {
         MessageBox.Show(args);
         return NestResult.Success;
      }

      public NestResult Shutdown() {
         return NestResult.Success;
      }
   }
}
