using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ItzWarty;

namespace nest_host {
   class Program {
      public static void Main(string[] args) {
         Console.SetIn(new StreamReader(Stream.Null));
         Console.SetOut(new StreamWriter(Stream.Null));
         Console.SetError(Console.Out);

         var stdin = Console.OpenStandardInput();
         var stdout = Console.OpenStandardOutput();

         using (var reader = new BinaryReader(stdin, Encoding.UTF8, true)) {
            const int kNestMagic = 0x5453454e;
            var magic = reader.ReadInt32();
            Trace.Assert(magic == kNestMagic);

            var pofContext = new PofContext().With(x => {
               x.MergeContext(new ExeggutorHostPofContext(3500)); // Note: Must reflect value in nest-host
            });
            var pofSerializer = new PofSerializer(pofContext);
            var bootstrapDto = pofSerializer.Deserialize<BootstrapDto>(stdin);

            var logDirectory = "C:/Dargon/logs";
            Directory.CreateDirectory(logDirectory);
            var outputWriter = new StreamWriter(new FileStream(Path.Combine(logDirectory, bootstrapDto.Name + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
            Console.SetIn(new StreamReader(Stream.Null));
            Console.SetOut(outputWriter);
            Console.SetError(outputWriter);

            Console.WriteLine("!");

            Application.ThreadException += HandleThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledAppDomainException;

            new EggHost().Run(bootstrapDto);
            GC.KeepAlive(stdin);
            GC.KeepAlive(stdout);
         }
      }

      private static void HandleThreadException(object sender, ThreadExceptionEventArgs e) {
         Console.Error.WriteLine("Unhandled Thread Exception");
         Console.Error.WriteLine(e.Exception);
      }

      private static void HandleUnhandledAppDomainException(object sender, UnhandledExceptionEventArgs e) {
         Console.Error.WriteLine("Unhandled Appdomain Exception");
         Console.Error.WriteLine(e.ExceptionObject);
      }
   }
}
