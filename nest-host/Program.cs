using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ItzWarty;

namespace nest_host {
   class Program {
      public static void Main(string[] args) {
         Console.SetIn(new StreamReader(Stream.Null));
         Console.SetOut(new StreamWriter(Stream.Null));

         var stdin = Console.OpenStandardInput();
         var stdout = Console.OpenStandardOutput();

         using (var reader = new BinaryReader(stdin, Encoding.UTF8, true)) {
            const int kNestMagic = 0x5453454e;
            var magic = reader.ReadInt32();
            Trace.Assert(magic == kNestMagic);

            var pofContext = new PofContext().With(x => {
               x.MergeContext(new ExeggutorHostPofContext(3500)); // Note: Must reflect value in nest-host
            });
            var pofSerializer = new PofSerializer(pofContext);
            var bootstrapDto = pofSerializer.Deserialize<BootstrapDto>(stdin);

            new EggHost().Run(bootstrapDto);
         }
      }
   }
}
