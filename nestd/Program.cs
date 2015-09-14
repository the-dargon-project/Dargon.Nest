using Castle.DynamicProxy;
using CommandLine;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services;
using Dargon.Services.Messaging;
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
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Dargon.Management;
using Dargon.Management.Server;
using Dargon.Services.Clustering;

namespace Dargon.Nest.Daemon {
   public static class Program {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      public class Options {
         [Option('p', "DspPort", DefaultValue = 21337,
                 HelpText = "Dargon Service Protocol Port")]
         public int DspPort { get; set; }

         [Option('m', "ManagementPort", DefaultValue = 21002,
                 HelpText = "Dargon Management Interface Port")]
         public int ManagementPort { get; set; }

         [Option("DspHeartBeatInterval", DefaultValue = 30000,
                 HelpText = "Dargon Service Protocol Heartbeat Interval")]
         public int DspHeartBeatInterval { get; set; }

         [Option('h', "HostPath", DefaultValue = "./../nest-host/nest-host.exe",
                 HelpText = "Path to nest host executable")]
         public string HostPath { get; set; }

         [Option('n', "NestPath", DefaultValue = null,
                 HelpText = "Path to dargon nest.")]
         public string NestPath { get; set; }

         [Option('c', "Command", DefaultValue = null,
                 HelpText = "Nest commands to execute when nest daemon has initialized.")]
         public string Command { get; set; }
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
         if (options.NestPath == null) {
            var nestDaemonDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            options.NestPath = nestDaemonDirectory.Parent.FullName;
         }

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

         // Common POF Context
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());               // 0 - 999
            x.MergeContext(new ExeggutorPofContext(3000));     // 3000 - 3499, must reflect value in nestd
            x.MergeContext(new ExeggutorHostPofContext(3500));
            x.MergeContext(new ManagementPofContext());
         });
         var pofSerializer = new PofSerializer(pofContext);

         // Pof Streams Dependencies
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         // construct libdsp dependencies
         ProxyGenerator proxyGenerator = new ProxyGenerator();
         var serviceClientFactory = new ServiceClientFactoryImpl(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         // construct libdsp local service node
         ServiceClient localServiceClient = serviceClientFactory.Local(options.DspPort, ClusteringRole.HostOnly);

         // construct dargon.management dependencies
         ITcpEndPoint managementServerEndpoint = tcpEndPointFactory.CreateAnyEndPoint(options.ManagementPort);
         var managementFactory = new ManagementFactoryImpl(collectionFactory, threadingProxy, networkingProxy, pofContext, pofSerializer);
         var localManagementServer = managementFactory.CreateServer(new ManagementServerConfiguration(managementServerEndpoint));

         // Nest-Host Dependencies
         ExecutorHostConfiguration executorHostConfiguration = new ExecutorHostConfigurationImpl(options.HostPath);
         HatchlingContextFactory hatchlingContextFactory = new HatchlingContextFactoryImpl(fileSystemProxy, pofSerializer, pofStreamsFactory, executorHostConfiguration);
         EggContextFactory eggContextFactory = new EggContextFactoryImpl(hatchlingContextFactory, processProxy);
         ExeggutorServiceImpl exeggutorService = new ExeggutorServiceImpl(options.NestPath, eggContextFactory);
         localManagementServer.RegisterInstance(new ExeggutorMob(exeggutorService));
         localServiceClient.RegisterService(exeggutorService, typeof(ExeggutorService));

         // Nest-Daemon Dependencies
         var nestDaemonService = new NestDaemonServiceImpl(exeggutorService);
         localServiceClient.RegisterService(nestDaemonService, typeof(NestDaemonService));
         logger.Info("Exposed nestd service.");

         AppDomain.CurrentDomain.ProcessExit += (s, e) => nestDaemonService.KillHatchlingsAndNest();

         nestDaemonService.Run();
         logger.Info("Shutting down nestd.");
      }

      private static void InitializeLogging() {
         var nestDaemonDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;
         var nestDirectory = nestDaemonDirectory.Parent;
         var nestLogsDirectory = new DirectoryInfo(Path.Combine(nestDirectory.FullName, "logs"));
         if (!nestLogsDirectory.Exists) {
            nestLogsDirectory.Create();
         }
         var nestLogFile = Path.Combine(nestLogsDirectory.FullName, "nestd.log");

         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget() {
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };
         Target consoleTarget = new ColoredConsoleTarget() {
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };
         Target fileTarget = new FileTarget {
            FileName = nestLogFile,
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };

#if !DEBUG
         debuggerTarget = new AsyncTargetWrapper(debuggerTarget);
         consoleTarget = new AsyncTargetWrapper(consoleTarget);
#else 
         AsyncTargetWrapper a; // Dummy variable for optimizing imports
#endif

         config.AddTarget("debugger", debuggerTarget);
         config.AddTarget("console", consoleTarget);
         config.AddTarget("logfile", fileTarget);

         var debuggerRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(debuggerRule);

         var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
         config.LoggingRules.Add(consoleRule);

         var fileLogRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
         config.LoggingRules.Add(fileLogRule);

         LogManager.Configuration = config;
      }
   }
}
