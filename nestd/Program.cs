using System;
using System.IO;
using System.Reflection;
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
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Threading;

namespace Dargon.Nest.Daemon {
   public static class Program {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      public class Options {
         [Option('p', "DspPort", DefaultValue = 21337,
                 HelpText = "Dargon Service Protocol Port")]
         public int DspPort { get; set; }

         [Option("DspHeartBeatInterval", DefaultValue = 30000,
                 HelpText = "Dargon Service Protocol Heartbeat Interval")]
         public int DspHeartBeatInterval { get; set; }

         [Option('h', "HostPath", DefaultValue = "./../nest-host/nest-host.exe",
                 HelpText = "Path to nest host executable")]
         public string HostPath { get; set; }

         [Option('n', "NestPath", DefaultValue = "C:/Dargon",
                 HelpText = "Path to dargon nest.")]
         public string NestPath { get; set; }
      }

      public static void Main(string[] args) {
         Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
         InitializeLogging();

         logger.Info("Logging Initialized.");
         var options = new Options();
         if (CommandLine.Parser.Default.ParseArguments(args, options)) {
            Run(options);
         } else {
            logger.Error("Invalid arguments.");
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
         IPofContext nestContext = new ExeggutorHostPofContext(0);
         IPofSerializer nestSerializer = new PofSerializer(nestContext);

         // Common POF Context
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());               // 0 - 999
            x.MergeContext(new ExeggutorPofContext(3000));     // 3000 - 3499, must reflect value in nestd
            x.MergeContext(new ExeggutorHostPofContext(3500)); // 3500 - 3999 
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

         logger.Info("Exposed nestd service.");
         var exitLatch = new CountdownEvent(1);
         exitLatch.Wait();
      }

      private static void InitializeLogging() {
         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget();
         Target consoleTarget = new ColoredConsoleTarget();

#if !DEBUG
         debuggerTarget = new AsyncTargetWrapper(debuggerTarget);
         consoleTarget = new AsyncTargetWrapper(consoleTarget);
#else 
         AsyncTargetWrapper a; // Dummy variable for optimizing imports
#endif

         config.AddTarget("debugger", debuggerTarget);
         config.AddTarget("console", consoleTarget);

         var debuggerRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(debuggerRule);

         var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
         config.LoggingRules.Add(consoleRule);

         LogManager.Configuration = config;
      }
   }
}
