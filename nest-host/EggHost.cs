using Dargon.Nest.Egg;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects.Streams;
using ItzWarty;
using ItzWarty.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;

namespace nest_host {
   public class EggHost : IEggHost {
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly PofStream pofStream;
      private readonly ICountdownEvent shutdownLatch = new CountdownEventProxy(1);

      public EggHost(PofStreamsFactory pofStreamsFactory, PofStream pofStream) {
         this.pofStreamsFactory = pofStreamsFactory;
         this.pofStream = pofStream;
      }

      public void Run(BootstrapDto bootstrapArguments) {
         Console.WriteLine($"Path = \"{bootstrapArguments.EggPath}\"; Name = \"{bootstrapArguments.Name}\"; PayloadLength = {bootstrapArguments.PayloadBytes.Length}.");

         string eggAssemblyPath;
         if (!TryGetEggAssemblyPath(bootstrapArguments, out eggAssemblyPath)) {
            Console.Error.WriteLine($"Could not find nest-main.dll in \"{bootstrapArguments.EggPath}\"!");
         } else {
            Dictionary<string, string> assemblyPathsByAssemblySimpleName;
            Dictionary<string, string> assemblyPathsByAssemblyFullName;
            GetAssemblyPathsByAssemblyName(bootstrapArguments, out assemblyPathsByAssemblySimpleName, out assemblyPathsByAssemblyFullName);

            AppDomain.CurrentDomain.AssemblyResolve += CreateCachedAssemblyResolveHandler(assemblyPathsByAssemblySimpleName, assemblyPathsByAssemblyFullName);
            INestApplicationEgg eggInstance = InstantiateNestApplicationEgg(eggAssemblyPath);
            var dispatcher = pofStreamsFactory.CreateDispatcher(pofStream);
            dispatcher.RegisterHandler<ShutdownDto>(dto => Console.WriteLine("Egg shutdown result: " + eggInstance.Shutdown() + "!"));
            dispatcher.RegisterShutdownHandler(() => eggInstance.Shutdown());
            var startResult = eggInstance.Start(new EggParameters(this, bootstrapArguments.Name, bootstrapArguments.PayloadBytes));
            dispatcher.Start();
            pofStream.Write(new BootstrapResultDto(startResult));
            Console.WriteLine("Egg started with " + startResult);

            shutdownLatch.Wait();
            Console.WriteLine("Main thread received shutdown signal.");
         }
      }

      private INestApplicationEgg InstantiateNestApplicationEgg(string eggAssemblyPath) {
         var eggAssembly = Assembly.LoadFile(eggAssemblyPath);
         var candidates = eggAssembly.GetExportedTypes().Where(FilterNestApplicationEggs).ToArray();
         if (candidates.Length != 1) {
            throw new InvalidOperationException("Could not select Nest entry point - too many candidates! Found " + candidates.Select(c => c.FullName).Join(", "));
         } else {
            var eggClass = candidates.First();
            return (INestApplicationEgg)Activator.CreateInstance(eggClass);
         }
      }

      private static bool TryGetEggAssemblyPath(BootstrapDto bootstrapArguments, out string eggStartPath) {
         eggStartPath = Path.Combine(bootstrapArguments.EggPath, "nest-main.dll");
         return File.Exists(eggStartPath);
      }

      private ResolveEventHandler CreateCachedAssemblyResolveHandler(Dictionary<string, string> assemblyPathsBySimpleName, Dictionary<string, string> assemblyPathsByFullName) {
         var loadedAssembliesByPath = new Dictionary<string, Assembly>();
         return (sender, e) => {
            Console.WriteLine("Trying to resolve: " + e.Name);
            string assemblyPath = assemblyPathsByFullName.GetValueOrDefault(e.Name);
            if (assemblyPath == null) {
               var simpleName = e.Name.Substring(0, e.Name.IndexOf(','));
               assemblyPath = assemblyPathsBySimpleName.GetValueOrDefault(simpleName);
            }

            if (assemblyPath == null) {
               return null;
            } else {
               var assembly = loadedAssembliesByPath.GetValueOrDefault(assemblyPath);
               if (assembly == null) {
                  assembly = Assembly.LoadFile(assemblyPath);
                  loadedAssembliesByPath.Add(assemblyPath, assembly);
               }
               return assembly;
            }
         };
      }

      private void GetAssemblyPathsByAssemblyName(BootstrapDto bootstrapArguments, out Dictionary<string, string> assemblyPathsBySimpleName, out Dictionary<string, string> assemblyPathsByFullName) {
         var loadableAssemblyPaths = Directory.GetFiles(bootstrapArguments.EggPath, "*", SearchOption.AllDirectories).Where(FilterLoadableAssemblyPaths);
         //         var additionalAssemblyPathsByAssemblyName = new Dictionary<string, string>();
         assemblyPathsBySimpleName = new Dictionary<string, string>();
         assemblyPathsByFullName = new Dictionary<string, string>();
         foreach (var assemblyPath in loadableAssemblyPaths) {
            try {
               var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
               assemblyPathsBySimpleName.Add(assemblyName.Name, assemblyPath);
               assemblyPathsByFullName.Add(assemblyName.FullName, assemblyPath);
            } catch (BadImageFormatException) {}
         }
      }

      private bool FilterLoadableAssemblyPaths(string arg) {
         return arg.EndsWithAny(new[] { "exe", "dll" }, StringComparison.OrdinalIgnoreCase);
      }

      public void Shutdown() {
         shutdownLatch.Signal();
      }

      private bool FilterNestApplicationEggs(Type type) {
         return type.GetInterfaces().Any(i => i.Name.Contains(nameof(INestApplicationEgg)));
      }
   }
}
