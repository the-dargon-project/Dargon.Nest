using Castle.DynamicProxy;
using CommandLine;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services;
using Dargon.Services.Messaging;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using System;
using System.IO;
using Dargon.Nest.Daemon;
using Dargon.Services.Clustering;

namespace dev_egg_runner {
   public class Options {
      [Option('p', "port", DefaultValue = 21337,
              HelpText = "Port of the nest cluster.")]
      public int NestPort { get; set; }

      [Option('c', "command", HelpText = "spawn-egg|kill-nest")]
      public string Command { get; set; }

      [Option('e', "egg", DefaultValue = null,
              HelpText = "Name of the Dargon Egg to run.")]
      public string EggName { get; set; }

      [Option('n', "name", DefaultValue = null,
              HelpText = "Name of the instance to run.")]
      public string InstanceName { get; set; }
   }

   public static class Program {
      public const int kHeartbeatIntervalMilliseconds = 1000;

      public static void Main(string[] args) {
         var options = new Options();
         if (!Parser.Default.ParseArguments(args, options)) {
            return;
         }

         IStreamFactory streamFactory = new StreamFactory();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);

         IPofContext pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());               // 0 - 999
            x.MergeContext(new ExeggutorPofContext(3000));     // 3000 - 3499, must reflect value in nestd
         });
         IPofSerializer pofSerializer = new PofSerializer(pofContext);

         ProxyGenerator proxyGenerator = new ProxyGenerator();
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);
         ServiceClientFactory serviceClientFactory = new ServiceClientFactoryImpl(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         var client = serviceClientFactory.Local(options.NestPort, ClusteringRole.GuestOnly);
         var exeggutor = client.GetService<ExeggutorService>();
         var nestDaemon = client.GetService<NestDaemonService>();

         switch (options.Command) {
            case "spawn-egg":
               SpawnEgg(pofSerializer, exeggutor, options);
               break;
            case "kill-nest": 
               nestDaemon.KillHatchlingsAndNest();
               break;
         }
      }

      private static void SpawnEgg(IPofSerializer pofSerializer, ExeggutorService exeggutor, Options options) {
         var ms = new MemoryStream();
         pofSerializer.Serialize(ms, (object)null);
         try {
            exeggutor.SpawnHatchling(options.EggName, new SpawnConfiguration {
               Arguments = ms.ToArray(),
               InstanceName = options.InstanceName
            });
         } catch (PortableException e) {
            Console.WriteLine(e.InnerException);
         }
      }
   }
}
