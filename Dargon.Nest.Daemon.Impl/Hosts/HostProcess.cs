using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects.Streams;
using Fody.Constructors;
using ItzWarty;
using ItzWarty.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Dargon.Nest.Daemon.Hosts {
   [RequiredFieldsConstructor]
   public class HostProcessFactory {
      private readonly IStreamFactory streamFactory = null;
      private readonly PofStreamsFactory pofStreamsFactory = null;
      private readonly DaemonConfiguration daemonConfiguration = null;

      public HostProcess CreateAndInitialize(EggContext eggContext, SpawnConfiguration spawnConfiguration) {
         var originalNestHostFileInfo = new FileInfo(daemonConfiguration.HostExecutablePath);
         var targetNestHostFileInfo = new FileInfo(Path.Combine(eggContext.Location, originalNestHostFileInfo.Name));

         var processStartInfo = new ProcessStartInfo() {
            FileName = targetNestHostFileInfo.FullName,
            Arguments = $"--name {spawnConfiguration.InstanceName}",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
         };

         var process = new Process {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
         };

         process.Start();

         var reader = streamFactory.CreateFromStream(process.StandardOutput.BaseStream).Reader;
         var writer = streamFactory.CreateFromStream(process.StandardInput.BaseStream).Writer;
         var pofStream = pofStreamsFactory.CreatePofStream(reader, writer);
         var pofDispatcher = pofStreamsFactory.CreateDispatcher(pofStream);

         return new HostProcess(eggContext, process, spawnConfiguration, pofStream, pofDispatcher).With(x => x.Initialize());
      }
   }

   public delegate void HostProcessExitedEventHandler(HostProcess process, EventArgs e);

   public class HostProcess {
      private readonly EggContext eggContext;
      private readonly Process process;
      private readonly SpawnConfiguration spawnConfiguration;
      private readonly PofStream pofStream;
      private readonly PofDispatcher pofDispatcher;

      private readonly AsyncManualResetEvent startResultLatch = new AsyncManualResetEvent();
      private NestResult startResult;

      public event HostProcessExitedEventHandler Exited;

      public HostProcess(EggContext eggContext, Process process, SpawnConfiguration spawnConfiguration, PofStream pofStream, PofDispatcher pofDispatcher) {
         this.eggContext = eggContext;
         this.process = process;
         this.spawnConfiguration = spawnConfiguration;
         this.pofStream = pofStream;
         this.pofDispatcher = pofDispatcher;
      }

      public Process Process => process;
      public SpawnConfiguration SpawnConfiguration => spawnConfiguration;
      public PofStream Stream => pofStream;
      public PofDispatcher Dispatcher => pofDispatcher;
      public async Task<NestResult> GetStartResultAsync() {
         await startResultLatch.WaitAsync();
         return startResult;
      }

      public void Initialize() {
         process.Exited += (s, e) => Exited?.Invoke(this, e);
         pofDispatcher.RegisterHandler<BootstrapResultDto>(HandleBootstrapResult);
         pofDispatcher.Start();

         SendMagic();
         Send(new BootstrapDto(
            spawnConfiguration.InstanceName,
            eggContext.Location,
            spawnConfiguration.Arguments)).Wait();
      }

      public async Task ShutdownAsync(ShutdownReason reason) {
         await Send(new ShutdownDto { Reason = reason });
      }

      private void SendMagic() {
         pofStream.Writer.BaseStream.Writer.Write(0x5453454e);
         pofStream.Writer.BaseStream.Flush();
      }

      private async Task Send(object x) {
         await pofStream.WriteAsync(x);
         pofStream.Writer.BaseStream.Flush();
      }

      private void HandleBootstrapResult(BootstrapResultDto x) {
         startResult = x.StartResult;
         startResultLatch.Set();
      }
   }
}