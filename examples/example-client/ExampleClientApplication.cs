using Dargon.Nest.Eggxecutor;
using Dargon.Ryu;
using Dargon.Services;
using Dargon.Services.Clustering;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Dargon.Nest.Eggs;

namespace example_client {
   public class ProgramRyuPackage : RyuPackageV1 {
      public ProgramRyuPackage() {
         Singleton<ExeggutorService>(ryu => ryu.Get<ServiceClient>().GetService<ExeggutorService>());
      }
   } 

   public class ExampleClientApplication : NestApplication {
      public static void Main() {
         new ExampleClientApplication().Start(null);
         new ManualResetEvent(false).WaitOne();
      }

      private HatchlingHost host;

      public NestResult Start(HatchlingParameters parameters) {
         host = parameters?.Host;

         var ryu = new RyuFactory().Create();
         ryu.Set<ClusteringConfiguration>(new ClusteringConfigurationImpl(
            IPAddress.Loopback,
            40000,
            ClusteringRole.HostOrGuest));
         ((RyuContainerImpl)ryu).Setup(true);
         Log("Successfully initialized example client.");
         new Thread(() => {
            Thread.Sleep(2500);
            Log("Sending killall signal.");
            var exeggutor = ryu.Get<ExeggutorService>();
            exeggutor.KillAllHatchlings(ShutdownReason.None);
            Log("Sending killall signal - done!");
         }) { IsBackground = true }.Start();
         return NestResult.Success;
      }

      public NestResult Shutdown(ShutdownReason reason) {
         Log($"Received shutdown signal with reason: {reason}.");
         host?.Shutdown();
         return NestResult.Success;
      }

      private void Log(string s) => Console.WriteLine("Client: " + s);
   }
}
