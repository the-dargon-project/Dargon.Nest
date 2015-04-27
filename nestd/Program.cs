using Castle.DynamicProxy;
using CommandLine;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services;
using Dargon.Services.Clustering.Host;
using Dargon.Services.PortableObjects;
using Dargon.Services.Server;
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

         [Option('c', "Command", DefaultValue = null,
                 HelpText = "Nest commands to execute when nest daemon has initialized.")]
         public string Command { get; set; }
      }

      public static void Main(string[] args) {
         Environment.CurrentDirectory = "C:/Dargon/nestd";// new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
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
         IPofContext nestContext = new ExeggutorHostPofContext(3500);
         IPofSerializer nestSerializer = new PofSerializer(nestContext);


         // Common POF Context
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());               // 0 - 999
            x.MergeContext(new ExeggutorPofContext(3000));     // 3000 - 3499, must reflect value in nestd
         });
         var pofSerializer = new PofSerializer(pofContext);

         // Pof Streams Dependencies
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         // construct libdsp dependencies
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory);

         ProxyGenerator proxyGenerator = new ProxyGenerator();
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory);
         IServiceClientFactory serviceClientFactory = new ServiceClientFactory(proxyGenerator, collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, invokableServiceContextFactory);
         // construct libdsp local service node
         IServiceClient localServiceClient = serviceClientFactory.CreateOrJoin(new ClusteringConfiguration(options.DspPort, options.DspHeartBeatInterval));

         // Nest-Host Dependencies
         ExecutorHostConfiguration executorHostConfiguration = new ExecutorHostConfigurationImpl(options.HostPath);
         HatchlingContextFactory hatchlingContextFactory = new HatchlingContextFactoryImpl(nestSerializer, executorHostConfiguration);
         EggContextFactory eggContextFactory = new EggContextFactoryImpl(hatchlingContextFactory, processProxy);
         ExeggutorService service = new ExeggutorServiceImpl(options.NestPath, eggContextFactory);
         localServiceClient.RegisterService(service, typeof(ExeggutorService));

         logger.Info("Exposed nestd service.");
         var exitLatch = new CountdownEvent(1);
         exitLatch.Wait();
      }

      private static void InitializeLogging() {
         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget() {
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };
         Target consoleTarget = new ColoredConsoleTarget() {
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

         var debuggerRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(debuggerRule);

         var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
         config.LoggingRules.Add(consoleRule);

         LogManager.Configuration = config;
      }
   }
}
