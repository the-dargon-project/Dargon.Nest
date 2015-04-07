using CommandLine;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using Dargon.Services;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
using Dargon.Services.Server.Phases;
using Dargon.Services.Server.Sessions;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Processes;
using ItzWarty.Threading;
using System.Threading;

namespace Dargon.Nest.Daemon {
   public static class Program {
      public class Options {
         [Option('p', "DspPort", DefaultValue = 21337,
                 HelpText = "Dargon Service Protocol Port")]
         public int DspPort { get; set; }

         [Option("DspHeartBeatInterval", DefaultValue = 30000,
                 HelpText = "Dargon Service Protocol Heartbeat Interval")]
         public int DspHeartBeatInterval { get; set; }

         [Option('h', "HostPath", DefaultValue = "nest-host.exe",
                 HelpText = "Path to nest host executable")]
         public string HostPath { get; set; }

         [Option('n', "NestPath", DefaultValue = "C:/Dargon",
                 HelpText = "Path to dargon nest.")]
         public string NestPath { get; set; }
      }

      public static void Main(string[] args) {
         var options = new Options();
         if (CommandLine.Parser.Default.ParseArguments(args, options)) {
            Run(options);
         }
      }

      public static void Run(Options options) {  
         // construct libwarty dependencies
         ICollectionFactory collectionFactory = new CollectionFactory();

         // construct libwarty-proxies dependencies
         IStreamFactory streamFactory = new StreamFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IProcessProxy processProxy = new ProcessProxy();

         // POF Dependencies
         IPofContext nestContext = new ExeggutorPofContext(0);
         IPofSerializer nestSerializer = new PofSerializer(nestContext);

         // Common POF Context
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());            // 0 - 999
            x.MergeContext(new ExeggutorPofContext(1000));  // 1000 - 1999
         });
         var pofSerializer = new PofSerializer(pofContext);

         // construct libdsp dependencies
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(collectionFactory, pofSerializer);
         IPhaseFactory phaseFactory = new PhaseFactory(collectionFactory, threadingProxy, networkingProxy, hostSessionFactory, pofSerializer);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, networkingProxy, phaseFactory);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         IServiceNodeFactory serviceNodeFactory = new ServiceNodeFactory(connectorFactory, serviceContextFactory, collectionFactory);

         // construct libdsp local service node
         IServiceConfiguration serviceConfiguration = new ServiceConfiguration(options.DspPort, options.DspHeartBeatInterval);
         IServiceNode localServiceNode = serviceNodeFactory.CreateOrJoin(serviceConfiguration);

         // Nest-Host Dependencies
         ExecutorHostConfiguration executorHostConfiguration = new ExecutorHostConfigurationImpl(options.HostPath);
         HatchlingContextFactory hatchlingContextFactory = new HatchlingContextFactoryImpl(nestSerializer, executorHostConfiguration);
         EggContextFactory eggContextFactory = new EggContextFactoryImpl(hatchlingContextFactory, processProxy);
         ExeggutorService service = new ExeggutorServiceImpl(options.NestPath, eggContextFactory);
         localServiceNode.RegisterService(service, typeof(ExeggutorService));

         var exitLatch = new CountdownEvent(1);
         exitLatch.Wait();
      }
   }
}
