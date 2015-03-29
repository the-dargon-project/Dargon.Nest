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
   public interface RemoteHostContextFactory {
      RemoteHostContext Create(string name, string eggPath);
   }

   public class RemoteHostContextFactoryImpl : RemoteHostContextFactory {
      private readonly IPofSerializer serializer;
      private readonly ExecutorHostConfiguration configuration;

      public RemoteHostContextFactoryImpl(IPofSerializer serializer, ExecutorHostConfiguration configuration) {
         this.serializer = serializer;
         this.configuration = configuration;
      }

      public RemoteHostContext Create(string name, string eggPath) {
         // command line arguments are solely for process list debugging.
         var args = new List<string>();
         if (name != null) {
            args.Add("--name " + name);
         }
         args.Add("\"" + eggPath + "\"");

         var hostProcess = Process.Start(
            new ProcessStartInfo(
               configuration.HostExecutablePath,
               args.Join(" ")
            ) {
               UseShellExecute = false,
               RedirectStandardError = true,
               RedirectStandardOutput = true,
               RedirectStandardInput = true
            }
         );

         var reader = new BinaryReaderWrapper(new StreamWrapper(hostProcess?.StandardOutput.BaseStream));
         var writer = new BinaryWriterWrapper(new StreamWrapper(hostProcess?.StandardInput.BaseStream));
         return new RemoteHostContextImpl(serializer, name, eggPath, hostProcess.ActLike<IProcess>(), reader, writer);
      }
   }
}