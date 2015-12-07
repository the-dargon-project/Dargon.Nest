using Dargon.Nest.Eggs;
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
   public class HatchlingHostImpl : HatchlingHost {
      private readonly ICountdownEvent shutdownLatch = new CountdownEventProxy(1);
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly PofStream pofStream;
      private readonly BootstrapDto bootstrapArguments;

      public HatchlingHostImpl(PofStreamsFactory pofStreamsFactory, PofStream pofStream, BootstrapDto bootstrapArguments) {
         this.pofStreamsFactory = pofStreamsFactory;
         this.pofStream = pofStream;
         this.bootstrapArguments = bootstrapArguments;
      }

      public void Run() {
         Console.WriteLine($"Path = \"{bootstrapArguments.EggPath}\"; Name = \"{bootstrapArguments.Name}\"; PayloadLength = {bootstrapArguments.PayloadBytes.Length}.");
         
         string eggAssemblyPath;
         if (!TryGetEggAssemblyPath(bootstrapArguments, out eggAssemblyPath)) {
            Console.Error.WriteLine($"Could not find nest-main.dll in \"{bootstrapArguments.EggPath}\"!");
         } else {
            Dictionary<string, string> assemblyPathsByAssemblySimpleName;
            Dictionary<string, string> assemblyPathsByAssemblyFullName;
            GetAssemblyPathsByAssemblyName(bootstrapArguments, out assemblyPathsByAssemblySimpleName, out assemblyPathsByAssemblyFullName);

            AppDomain.CurrentDomain.AssemblyResolve += CreateCachedAssemblyResolveHandler(assemblyPathsByAssemblySimpleName, assemblyPathsByAssemblyFullName);
            NestApplication instance = InstantiateNestApplicationEgg(eggAssemblyPath);
            var dispatcher = pofStreamsFactory.CreateDispatcher(pofStream);
            dispatcher.RegisterHandler<ShutdownDto>(dto => Console.WriteLine("Egg shutdown result: " + instance.Shutdown(dto.Reason) + "!"));
            dispatcher.RegisterShutdownHandler(() => instance.Shutdown(ShutdownReason.HostKilled));
            var startResult = instance.Start(new HatchlingParameters(this, bootstrapArguments.Name, bootstrapArguments.PayloadBytes));
            dispatcher.Start();
            pofStream.Write(new BootstrapResultDto(startResult));
            Console.WriteLine("Egg started with " + startResult);

            shutdownLatch.Wait();
            Console.WriteLine("Main thread received shutdown signal.");
         }
      }

      private NestApplication InstantiateNestApplicationEgg(string eggAssemblyPath) {
         var eggAssembly = Assembly.LoadFile(eggAssemblyPath);
         var candidates = eggAssembly.GetExportedTypes().Where(FilterNestApplicationEggs).ToArray();
         if (candidates.Length != 1) {
            throw new InvalidOperationException("Could not select Nest entry point - too many candidates! Found " + candidates.Select(c => c.FullName).Join(", "));
         } else {
            var eggClass = candidates.First();
            return (NestApplication)Activator.CreateInstance(eggClass);
         }
      }

      private static bool TryGetEggAssemblyPath(BootstrapDto bootstrapArguments, out string eggStartPath) {
         var pathWithoutExtension = Path.Combine(bootstrapArguments.EggPath, new DirectoryInfo(bootstrapArguments.EggPath).Name);
         if (File.Exists(eggStartPath = pathWithoutExtension + ".dll")) return true;
         if (File.Exists(eggStartPath = pathWithoutExtension + ".exe")) return true;
         return false;
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
         var bannedSimpleNames = new HashSet<string>();
         foreach (var assemblyPath in loadableAssemblyPaths) {
            try {
               var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
               var simpleName = assemblyName.Name;
               if (!bannedSimpleNames.Contains(simpleName)) {
                  if (assemblyPathsBySimpleName.ContainsKey(simpleName)) {
                     assemblyPathsBySimpleName.Remove(simpleName);
                     bannedSimpleNames.Add(simpleName);
                  } else {
                     assemblyPathsBySimpleName.Add(simpleName, assemblyPath);
                  }
               }

               assemblyPathsByFullName.Add(assemblyName.FullName, assemblyPath);
            } catch (BadImageFormatException) {}
         }
      }

      private bool FilterLoadableAssemblyPaths(string arg) {
         return arg.EndsWithAny(new[] { "exe", "dll" }, StringComparison.OrdinalIgnoreCase);
      }

      public void Shutdown() {
         Shutdown(ShutdownReason.None);
      }

      public void Shutdown(ShutdownReason reason) {
         shutdownLatch.Signal();
      }

      public void SetRestartToken(HatchlingParameters parameters) {
         var restartDirectoryPath = Path.Combine(bootstrapArguments.EggPath, "..", "..", "..", "restart");
         Directory.CreateDirectory(restartDirectoryPath);

         var restartFilePath = Path.Combine(restartDirectoryPath, parameters.InstanceName);
         using (var fs = File.OpenWrite(restartFilePath))
         using (var stream = pofStreamsFactory.CreatePofStream(fs)) {
            var dto = new BootstrapDto(
               parameters.InstanceName,
               bootstrapArguments.EggPath,
               parameters.Arguments);
            stream.Write(dto);
         }
      }

      private bool FilterNestApplicationEggs(Type type) {
         return type.GetInterfaces().Any(i => i.Name.Contains(nameof(NestApplication)));
      }
   }
}
