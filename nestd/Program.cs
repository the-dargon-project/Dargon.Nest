using CommandLine;
using Dargon.Management.Server;
using Dargon.Ryu;
using Dargon.Services;
using Dargon.Services.Clustering;
using ItzWarty;
using ItzWarty.Networking;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;
using System.Net;
using System.Reflection;

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

         [Option('h', "HostPath", DefaultValue = "./../nest-host/nest-host.exe",
                 HelpText = "Path to nest host executable")]
         public string HostPath { get; set; }

         [Option('n', "NestsPath", DefaultValue = "./../..",
                 HelpText = "Path to `nests` directory.")]
         public string NestPath { get; set; }
      }

      public static void Main(string[] args) {
         Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
         InitializeLogging();

         logger.Info("Logging Initialized.");
         var options = new Options();
         if (Parser.Default.ParseArguments(args, options)) {
            Run(options);
         } else {
            logger.Error("Invalid arguments.");
         }
      }

      public static void Run(Options options) {
         var ryu = new RyuFactory().Create();
         ryu.Touch<ItzWartyCommonsRyuPackage>();
         ryu.Touch<ItzWartyProxiesRyuPackage>();

         // Configure Dargon.Services
         ryu.Set<ClusteringConfiguration>(new ClusteringConfigurationImpl(
            IPAddress.Loopback,
            options.DspPort,
            ClusteringRole.HostOrGuest));

         // Configure Dargon.Management
         var managementServerEndpoint = ryu.Get<INetworkingProxy>().CreateAnyEndPoint(options.ManagementPort);
         ryu.Set<IManagementServerConfiguration>(new ManagementServerConfiguration(managementServerEndpoint));

         // Configure Dargon.Nest.Daemon
         ryu.Set<DaemonConfiguration>(new DaemonConfiguration {
            HostExecutablePath = options.HostPath,
            NestsPath = options.NestPath
         });

         ((RyuContainerImpl)ryu).Setup(true);

         logger.Info("..");

         ryu.Get<InternalNestDaemonService>().WaitForShutdown();
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
