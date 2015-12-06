using System;
using System.Net;
using System.Threading;
using Dargon.Nest.Eggs;
using Dargon.Ryu;
using Dargon.Services;
using Dargon.Services.Clustering;

namespace example_daemon {
   public class ExampleApplicationEgg : INestApplicationEgg {
      private IEggHost host;

      public NestResult Start(IEggParameters parameters) {
         host = parameters?.Host;

         var ryu = new RyuFactory().Create();
         ryu.Set<ClusteringConfiguration>(new ClusteringConfigurationImpl(
            IPAddress.Any, 
            40000,
            ClusteringRole.HostOrGuest));
         ((RyuContainerImpl)ryu).Setup(true);
         Log("Successfully initialized example daemon.");
         return NestResult.Success;
      }

      public NestResult Shutdown(ShutdownReason reason) {
         Log("Received shutdown signal.");
         host?.Shutdown();
         return NestResult.Success;
      }

      private void Log(string s) => Console.WriteLine("Daemon: " + s);
   }
}
