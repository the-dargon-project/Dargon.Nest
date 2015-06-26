using Dargon.Nest.Egg;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects.Streams;
using ItzWarty;
using ItzWarty.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace nest_host {
   public class EggHost : IEggHost {
      private readonly PofStream pofStream;
      private readonly ICancellationTokenSource shutdownCancellationTokenSource;

      public EggHost(PofStream pofStream, ICancellationTokenSource shutdownCancellationTokenSource) {
         this.pofStream = pofStream;
         this.shutdownCancellationTokenSource = shutdownCancellationTokenSource;
      }

      public void Run(BootstrapDto bootstrapArguments) {
         Console.WriteLine($"Path = \"{bootstrapArguments.EggPath}\"; Name = \"{bootstrapArguments.Name}\"; PayloadLength = {bootstrapArguments.PayloadBytes.Length}.");

         string eggAssemblyPath;
         if (!TryGetEggAssemblyPath(bootstrapArguments, out eggAssemblyPath)) {
            Console.Error.WriteLine($"Could not find nest-main.dll in \"{bootstrapArguments.EggPath}\"!");
         } else {
            var assemblyPathsByAssemblyName = GetAssemblyPathsByAssemblyName(bootstrapArguments);
            AppDomain.CurrentDomain.AssemblyResolve += CreateCachedAssemblyResolveHandler(assemblyPathsByAssemblyName);
            INestApplicationEgg eggInstance = InstantiateNestApplicationEgg(eggAssemblyPath);
            var startResult = eggInstance.Start(new EggParameters(this, bootstrapArguments.Name, bootstrapArguments.PayloadBytes));
            Console.WriteLine("Egg started with " + startResult);
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

      private ResolveEventHandler CreateCachedAssemblyResolveHandler(Dictionary<string, string> assemblyPathsByAssemblyName) {
         var loadedAssembliesByPath = new Dictionary<string, Assembly>();
         return (sender, e) => {
            string assemblyPath = assemblyPathsByAssemblyName.GetValueOrDefault(e.Name);
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

      private Dictionary<string, string> GetAssemblyPathsByAssemblyName(BootstrapDto bootstrapArguments) {
         var loadableAssemblyPaths = Directory.GetFiles(bootstrapArguments.EggPath, "*", SearchOption.AllDirectories).Where(FilterLoadableAssemblyPaths);
         var additionalAssemblyPathsByAssemblyName = new Dictionary<string, string>();
         foreach (var assemblyPath in loadableAssemblyPaths) {
            try {
               var assemblyName = AssemblyName.GetAssemblyName(assemblyPath).FullName;
               additionalAssemblyPathsByAssemblyName.Add(assemblyName, Path.GetFullPath(assemblyPath));
            } catch (BadImageFormatException) {}
         }
         return additionalAssemblyPathsByAssemblyName;
      }

      private bool FilterLoadableAssemblyPaths(string arg) {
         return arg.EndsWithAny(new[] { "exe", "dll" }, StringComparison.OrdinalIgnoreCase);
      }

      public void Shutdown() {
         shutdownCancellationTokenSource.Cancel();
      }

      private bool FilterNestApplicationEggs(Type type) {
         return type.GetInterfaces().Any(i => i.Name.Contains(nameof(INestApplicationEgg)));
      }
   }
}
