using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using ImpromptuInterface;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Processes;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Dargon.Nest.Exeggutor {
   public interface HatchlingContextFactory {
      HatchlingContext Create(string name, IDargonEgg egg);
   }

   public class HatchlingContextFactoryImpl : HatchlingContextFactory {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IPofSerializer nestSerializer;
      private readonly PofStreamsFactory pofStreamsFactory;
      private readonly ExecutorHostConfiguration configuration;

      public HatchlingContextFactoryImpl(IFileSystemProxy fileSystemProxy, IPofSerializer nestSerializer, PofStreamsFactory pofStreamsFactory, ExecutorHostConfiguration configuration) {
         this.fileSystemProxy = fileSystemProxy;
         this.nestSerializer = nestSerializer;
         this.pofStreamsFactory = pofStreamsFactory;
         this.configuration = configuration;
      }

      public HatchlingContext Create(string name, IDargonEgg egg) {
         var eggPath = egg.Location;

         logger.Info($"Spawning hatchling of name {name} and path {eggPath}!");
         logger.Info("nest-host is located at: " + configuration.HostExecutablePath);

         Guid instanceId = Guid.NewGuid();

         // command line arguments are solely for process list debugging.
         var args = new List<string>();
         if (name != null) {
            args.Add("--name " + name);
         }
         args.Add("\"" + eggPath + "\"");

         var nestHostFileInfo = new FileInfo(configuration.HostExecutablePath);
         var nestHostEggDirectory = nestHostFileInfo.Directory;
         var nestHostNeighboringAssemblies = from filePath in nestHostEggDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                                             let extension = filePath.Extension
                                             where extension.EndsWithAny(new[] { ".exe", ".dll" }, StringComparison.OrdinalIgnoreCase)
                                             select filePath;
         
         foreach (var neighboringAssemblyInfo in nestHostNeighboringAssemblies) {
            try {
               var relativePath = neighboringAssemblyInfo.FullName.Substring(nestHostEggDirectory.FullName.Length + 1);
               var eggAssemblyPath = Path.Combine(eggPath, relativePath);

               if (egg.Files.None(x => x.InternalPath.Equals(relativePath, StringComparison.OrdinalIgnoreCase))) {
                  File.Copy(neighboringAssemblyInfo.FullName, eggAssemblyPath, true);
               }
            } catch (IOException) {
               // An instance of the egg is probably already running.
            }
         }

         // Copy app.config
         string nestMainAppConfigPath = Path.Combine(eggPath, "nest-main.dll.config");
         string nestHostAppConfigPath = Path.Combine(eggPath, nestHostFileInfo.Name + ".config");
         if (File.Exists(nestMainAppConfigPath)) {
            File.Copy(nestMainAppConfigPath, nestHostAppConfigPath, true);
         }

         var eggNestHostPath = Path.Combine(eggPath, nestHostFileInfo.Name);
         logger.Info("Copied nest-host to " + eggNestHostPath);
         var processStartInfo = new ProcessStartInfo() {
            FileName = eggNestHostPath,
            Arguments = args.Join(" "),
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
         };
         var hostProcess = new Process().With(x => { x.StartInfo = processStartInfo; });
         hostProcess.EnableRaisingEvents = true;

         logger.Info("Starting nest-host process...");
         hostProcess.Start();
         logger.Info("nest-host process has started!");

         var reader = new BinaryReaderWrapper(new StreamWrapper(hostProcess.StandardOutput.BaseStream));
         var writer = new BinaryWriterWrapper(new StreamWrapper(hostProcess.StandardInput.BaseStream));

         writer.Write(0x5453454e);
         writer.Flush();

         var pofStream = pofStreamsFactory.CreatePofStream(reader, writer);
         var pofDispatcher = pofStreamsFactory.CreateDispatcher(pofStream);

         var context = new HatchlingContextImpl(instanceId, name, eggPath, hostProcess.ActLike<IProcess>(), pofStream, pofDispatcher, reader, writer);
         context.Initialize();
         return context;
      }
   }
}