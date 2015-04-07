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

         var hostProcess = Process.Start(
            new ProcessStartInfo(
               Path.GetFullPath(configuration.HostExecutablePath),
               args.Join(" ")
            ) {
               UseShellExecute = false,
               RedirectStandardError = true,
               RedirectStandardOutput = true,
               RedirectStandardInput = true
            }
         );
         Console.WriteLine("!B");

         var reader = new BinaryReaderWrapper(new StreamWrapper(hostProcess?.StandardOutput.BaseStream));
         var writer = new BinaryWriterWrapper(new StreamWrapper(hostProcess?.StandardInput.BaseStream));
         var context = new HatchlingContextImpl(nestSerializer, instanceId, name, eggPath, hostProcess.ActLike<IProcess>(), reader, writer);
         context.Initialize();
         return context;
      }
   }
}