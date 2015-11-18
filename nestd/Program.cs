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
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Daemon.Init;
using ItzWarty.IO;

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

         [Option("WorkingDirectory", DefaultValue = null,
                 HelpText = "Overrides the application's working directory.")]
         public string WorkingDirectory { get; set; }

         [Option('h', "HostPath", DefaultValue = "./../nest-host/nest-host.exe",
                 HelpText = "Path to nest host executable")]
         public string HostPath { get; set; }

         [Option('n', "NestsPath", DefaultValue = "./../..",
                 HelpText = "Path to `nests` directory.")]
         public string NestPath { get; set; }

         [Option('s', "StagePath", DefaultValue = "./../../../stage",
                 HelpText = "Path to `stage` directory.")]
         public string StagePath { get; set; }
      }

      public static void Main(string[] args) {
         var options = new Options();
         bool argumentsValid = Parser.Default.ParseArguments(args, options);
         if (!string.IsNullOrWhiteSpace(options.WorkingDirectory)) {
            Environment.CurrentDirectory = options.WorkingDirectory;
         }
         InitializeLogging();
         logger.Info("Logging Initialized.");

         if (argumentsValid) {
            Run(options);
         } else {
            logger.Error("Invalid arguments.");
         }
      }

      public static void Run(Options options) {
         logger.Info("Initializing with working directory: " + Environment.CurrentDirectory);
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
         var daemonConfiguration = new DaemonConfiguration {
            HostExecutablePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, options.HostPath)),
            NestsPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, options.NestPath)),
            StagePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, options.StagePath))
         };
         ryu.Set<DaemonConfiguration>(daemonConfiguration);

         ((RyuContainerImpl)ryu).Setup(true);

         logger.Info($"nestd initialized. dmi: {options.ManagementPort}. services: {options.DspPort}. Daemon configuration: {daemonConfiguration}");

         var initScriptRunner = ryu.Get<InitScriptRunner>();
         foreach (var nest in ryu.Get<ReadableNestDirectory>().EnumerateNests()) {
            initScriptRunner.RunNestInitializationScript(nest);
         }

         ryu.Get<NestDaemonService>().WaitForShutdown();
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
