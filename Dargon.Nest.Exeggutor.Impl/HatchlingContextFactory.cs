using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Dargon.PortableObjects;
using ImpromptuInterface;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public interface HatchlingContextFactory {
      HatchlingContext Create(string name, string eggPath);
   }

   public class HatchlingContextFactoryImpl : HatchlingContextFactory {
      private readonly IPofSerializer nestSerializer;
      private readonly ExecutorHostConfiguration configuration;

      public HatchlingContextFactoryImpl(IPofSerializer nestSerializer, ExecutorHostConfiguration configuration) {
         this.nestSerializer = nestSerializer;
         this.configuration = configuration;
      }

      public HatchlingContext Create(string name, string eggPath) {
         Guid instanceId = Guid.NewGuid();

         // command line arguments are solely for process list debugging.
         var args = new List<string>();
         if (name != null) {
            args.Add("--name " + name);
         }
         args.Add("\"" + eggPath + "\"");

         Console.WriteLine("!A");
         var processStartInfo = new ProcessStartInfo() {
            FileName = Path.GetFullPath(configuration.HostExecutablePath),
            Arguments = args.Join(" "),
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
         };
         var hostProcess = new Process().With(x => { x.StartInfo = processStartInfo; });
         hostProcess.EnableRaisingEvents = true;
         Console.WriteLine(processStartInfo.FileName);

         Console.WriteLine("!B");
         hostProcess.Start();
         Console.WriteLine("!C");

         var reader = new BinaryReaderWrapper(new StreamWrapper(hostProcess.StandardOutput.BaseStream));
         var writer = new BinaryWriterWrapper(new StreamWrapper(hostProcess.StandardInput.BaseStream));
//         writer = new BinaryWriterWrapper(new FileStreamWrapper(File.Open("C:/Dargon/out2.txt", FileMode.Create, FileAccess.Write, FileShare.None)));

         writer.Write(0x5453454e);

         var context = new HatchlingContextImpl(nestSerializer, instanceId, name, eggPath, hostProcess.ActLike<IProcess>(), reader, writer);
         context.Initialize();
         return context;
      }
   }
}