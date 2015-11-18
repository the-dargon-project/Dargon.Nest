using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace nest_host {
   class Program {
      private const int kNestMagic = 0x5453454e;

      public static void Main(string[] args) {
         // Null all input and output so we don't send corrupt bits to parent process.
         Console.SetIn(new StreamReader(Stream.Null));
         Console.SetOut(new StreamWriter(Stream.Null));
         Console.SetError(Console.Out);

         // Initialize Dependencies
         var streamFactory = new StreamFactory();
         var threadingFactory = new ThreadingFactory();
         var synchronizationFactory = new SynchronizationFactory();
         var threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new ExeggutorHostPofContext()); // Note: Must reflect value in nest-host
         });
         var pofSerializer = new PofSerializer(pofContext);
         var pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         // Open I/O streams to parent process
         var standardInput = streamFactory.CreateFromStream(Console.OpenStandardInput(), true);
         var standardOutput = streamFactory.CreateFromStream(Console.OpenStandardOutput(), true);

         var magic = standardInput.Reader.ReadInt32();
         Trace.Assert(magic == kNestMagic);

         var pofStream = pofStreamsFactory.CreatePofStream(standardInput.Reader, standardOutput.Writer);
         var bootstrapDto = pofStream.Read<BootstrapDto>();

         // Redirect nest-host (and thus hatchling) output to a log file.
         var nestHostDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
         var nestDirectory = nestHostDirectory.Parent;
         var logDirectory = Path.Combine(nestDirectory.FullName, "logs");
         Directory.CreateDirectory(logDirectory);
         var outputWriter = new StreamWriter(new FileStream(Path.Combine(logDirectory, bootstrapDto.Name + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
         Console.SetIn(new StreamReader(Stream.Null));
         Console.SetOut(outputWriter);
         Console.SetError(outputWriter);

         ConfigureNLog();

         // Dump crash info to our log file as well.
         Application.ThreadException += HandleThreadException;
         Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
         AppDomain.CurrentDomain.UnhandledException += HandleUnhandledAppDomainException;

         new EggHost(pofStreamsFactory, pofStream).Run(bootstrapDto);

         GC.KeepAlive(standardInput);
         GC.KeepAlive(standardOutput);
      }

      private static void HandleThreadException(object sender, ThreadExceptionEventArgs e) {
         Console.WriteLine("!");
         Console.WriteLine("!");
         Console.WriteLine("!");
         Console.WriteLine("!");
         Console.Error.WriteLine("Unhandled Thread Exception");
         Console.Error.WriteLine(e.Exception);
      }

      private static void HandleUnhandledAppDomainException(object sender, UnhandledExceptionEventArgs e) {
         Console.WriteLine("!!");
         Console.WriteLine("!!");
         Console.WriteLine("!!");
         Console.WriteLine("!!");
         Console.Error.WriteLine("Unhandled Appdomain Exception");
         Console.Error.WriteLine(e.ExceptionObject);
      }


      private static void ConfigureNLog() {
         var nlogLayout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";
         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget { Layout = nlogLayout };
         Target consoleTarget = new ConsoleTarget { Layout = nlogLayout }; //ColoredConsoleTarget();

#if !DEBUG
         debuggerTarget = new AsyncTargetWrapper(debuggerTarget);
         consoleTarget = new AsyncTargetWrapper(consoleTarget);
#else
         AsyncTargetWrapper a; // Placeholder for optimizing imports
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
