using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using CommandLine;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.Services.Client;
using Dargon.Services.PortableObjects;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace dev_egg_runner {
   public class Options {
      [Option('e', "egg", DefaultValue = "dev-egg-example",
              HelpText = "Name of the Dargon Egg to run.")]
      public string EggName { get; set; }

      [Option('n', "name", DefaultValue = null,
              HelpText = "Name of the instance to run.")]
      public string InstanceName { get; set; }
   }

   public static class Program {
      public const int kDaemonPort = 21337;

      public static void Main(string[] args) {
         var options = new Options();
         if (!CommandLine.Parser.Default.ParseArguments(args, options)) {
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

         IPofContext pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());               // 0 - 999
            x.MergeContext(new ExeggutorPofContext(3000));     // 3000 - 3499, must reflect value in nestd
         });
         IPofSerializer pofSerializer = new PofSerializer(pofContext);

         ProxyGenerator proxyGenerator = new ProxyGenerator();
         IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(proxyGenerator);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         IInvocationStateFactory invocationStateFactory = new InvocationStateFactory(threadingProxy);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, socketFactory, invocationStateFactory, pofSerializer);
         IServiceClientFactory serviceClientFactory = new ServiceClientFactory(collectionFactory, serviceProxyFactory, serviceContextFactory, connectorFactory);
         ITcpEndPoint endpoint = new TcpEndPoint(new IPEndPoint(IPAddress.Loopback, kDaemonPort));
         var client = serviceClientFactory.Create(endpoint);
         var exeggutor = client.GetService<ExeggutorService>();
         var ms = new MemoryStream();
         pofSerializer.Serialize(ms, (object)"test");
         try {
            exeggutor.SpawnHatchling(options.EggName, new SpawnConfiguration {
               Arguments = ms.ToArray(),
               InstanceName = options.InstanceName
            });
         } catch (PortableException e) {
            Console.WriteLine(e.InnerException);
         }
         while (true) ;
      }
   }
}
