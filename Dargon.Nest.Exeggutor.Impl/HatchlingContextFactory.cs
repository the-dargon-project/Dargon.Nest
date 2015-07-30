using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using ImpromptuInterface;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Processes;
using NLog;

namespace Dargon.Nest.Exeggutor {
   public interface HatchlingContextFactory {
      HatchlingContext Create(string name, string eggPath);
   }

   public class HatchlingContextFactoryImpl : HatchlingContextFactory {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IPofSerializer nestSerializer;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly ExecutorHostConfiguration configuration;

      public HatchlingContextFactoryImpl(IPofSerializer nestSerializer, PofStreamsFactory pofStreamsFactory, ExecutorHostConfiguration configuration) {
         this.nestSerializer = nestSerializer;
         this.pofStreamsFactory = pofStreamsFactory;
         this.configuration = configuration;
      }

      public HatchlingContext Create(string name, string eggPath) {
         logger.Info($"Spawning hatchling of name {name} and path {eggPath}!");
         logger.Info("nest-host is located at: " + configuration.HostExecutablePath);

         Guid instanceId = Guid.NewGuid();

         // command line arguments are solely for process list debugging.
         var args = new List<string>();
         if (name != null) {
            args.Add("--name " + name);
         }
         args.Add("\"" + eggPath + "\"");

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

         logger.Info("Starting nest-host process...");
         hostProcess.Start();
         logger.Info("nest-host process has started!");

         var reader = new BinaryReaderWrapper(new StreamWrapper(hostProcess.StandardOutput.BaseStream));
         var writer = new BinaryWriterWrapper(new StreamWrapper(hostProcess.StandardInput.BaseStream));

         writer.Write(0x5453454e);
         writer.Flush();

         var pofStream = pofStreamsFactory.CreatePofStream(reader, writer);
         var pofDispatcher = pofStreamsFactory.CreateDispatcher(pofStream);

         var context = new HatchlingContextImpl(instanceId, name, eggPath, hostProcess.ActLike<IProcess>(), pofStream, pofDispatcher, reader, writer);
         context.Initialize();
         return context;
      }
   }
}