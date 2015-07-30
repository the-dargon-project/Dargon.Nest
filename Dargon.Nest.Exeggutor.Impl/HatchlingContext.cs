using Dargon.PortableObjects;
using ItzWarty.IO;
using ItzWarty.Processes;
using System;
using System.Threading;
using Dargon.Nest.Egg;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects.Streams;
using NLog;

namespace Dargon.Nest.Exeggutor {
   public interface HatchlingContext {
      Guid InstanceId { get; }
      string Name { get; }
      string Path { get; }
      NestResult StartResult { get; }

      event EventHandler Exited;

      void StartBootstrap(byte[] arguments);
      void Shutdown();
   }

   public class HatchlingContextImpl : HatchlingContext {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ManualResetEvent startResultLatch = new ManualResetEvent(false);

      private readonly Guid instanceId;
      private readonly string name;
      private readonly string eggPath;
      private readonly IProcess hatchlingProcess;
      private readonly PofStream pofStream;
      private readonly PofDispatcher pofDispatcher;
      private readonly IBinaryReader reader;
      private readonly IBinaryWriter writer;
      private NestResult startResult;

      public HatchlingContextImpl(Guid instanceId, string name, string eggPath, IProcess hatchlingProcess, PofStream pofStream, PofDispatcher pofDispatcher, IBinaryReader reader, IBinaryWriter writer) {
         this.instanceId = instanceId;
         this.name = name;
         this.eggPath = eggPath;
         this.hatchlingProcess = hatchlingProcess;
         this.pofStream = pofStream;
         this.pofDispatcher = pofDispatcher;
         this.reader = reader;
         this.writer = writer;
      }

      public Guid InstanceId => instanceId;
      public string Name => name;
      public string Path => eggPath;
      public NestResult StartResult => GetStartResult();

      public void Initialize() {
         pofDispatcher.RegisterHandler<BootstrapResultDto>(HandleBootstrapResultDto);
         pofDispatcher.Start();
      }

      public void StartBootstrap(byte[] arguments) {
         logger.Info("Writing bootstrap dto!");
         pofStream.Write(new BootstrapDto(name, eggPath, arguments));
         writer.Flush();
         logger.Info("Wrote bootstrap dto");
      }

      public void Shutdown() {
         pofStream.Write(new ShutdownDto());
         writer.Flush();
         logger.Info("Wrote shutdown dto!");
      }

      private NestResult GetStartResult() {
         startResultLatch.WaitOne();
         return startResult; 
      }

      private void HandleBootstrapResultDto(BootstrapResultDto x) {
         startResult = x.StartResult;
         startResultLatch.Set();
      }

      public event EventHandler Exited { add { hatchlingProcess.Exited += value; } remove { hatchlingProcess.Exited -= value; } }
   }
}