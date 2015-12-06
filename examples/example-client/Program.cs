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

   public class Program : INestApplicationEgg {
      public static void Main() {
         new Program().Start(null);
         new ManualResetEvent(false).WaitOne();
      }

      private IEggHost host;

      public NestResult Start(IEggParameters parameters) {
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
            Log("Sending killall + update signal.");
            var exeggutor = ryu.Get<ExeggutorService>();
            exeggutor.KillHatchlingsAndUpdateAllPackages();
            Log("Sending killall + update signal - done!");
         }) { IsBackground = true }.Start();
         return NestResult.Success;
      }

      public NestResult Shutdown(ShutdownReason reason) {
         Log("Received shutdown signal.");
         host?.Shutdown();
         return NestResult.Success;
      }

      private void Log(string s) => Console.WriteLine("Client: " + s);
   }
}
