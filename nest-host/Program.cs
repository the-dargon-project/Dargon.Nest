using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using System;
using System.IO;
using System.Linq;

namespace nest_host {
   class Program {
      public static void Main(string[] args) {
         //var stdin = Console.OpenStandardInput();
         //var stdout = Console.OpenStandardOutput();
         //
         //Console.SetIn(new StreamReader(Stream.Null));
         //Console.SetOut(new StreamWriter(Stream.Null));
         //
         var pofContext = new ExeggutorHostPofContext(1000); // Note: Must reflect value in nest-host
         var pofSerializer = new PofSerializer(pofContext);
         //
         //var bootstrapDto = pofSerializer.Deserialize<BootstrapDto>(stdin);

         var ms = new MemoryStream();
         pofSerializer.Serialize(ms, (object)"test");

         var bootstrapDto = new BootstrapDto(null, @"C:\Dargon\dev-egg-example", ms.ToArray());

         new EggHost().Run(bootstrapDto);
      }
   }
}
