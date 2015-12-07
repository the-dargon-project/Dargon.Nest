using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects.Streams;
using Fody.Constructors;
using ItzWarty.IO;
using NLog;

namespace Dargon.Nest.Daemon.Restart {
   [RequiredFieldsConstructor]
   public class RestartSignalService {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly IFileSystemProxy fileSystemProxy = null;
      private readonly PofStreamsFactory pofStreamsFactory = null;
      private readonly ManageableDeployment localDeployment = null;
      private readonly HatchlingSpawner hatchlingSpawner = null;
      private string restartDirectoryPath;

      public void Initialize() {
         restartDirectoryPath = Path.Combine(localDeployment.Location, "restart");
         fileSystemProxy.PrepareDirectory(restartDirectoryPath);
      }

      public void ProcessRestartSignals() {
         fileSystemProxy.PrepareDirectory(restartDirectoryPath);
         foreach (var signalPath in Directory.EnumerateFiles(restartDirectoryPath)) {
            logger.Info("Found restart signal at " + signalPath);
            try {
               using (var fs = fileSystemProxy.OpenFile(signalPath, FileMode.Open, FileAccess.Read))
               using (var pofStream = pofStreamsFactory.CreatePofStream(fs)) {
                  var bootstrapDto = pofStream.Read<BootstrapDto>();
                  var eggDirectoryInfo = new DirectoryInfo(bootstrapDto.EggPath);
                  var eggFullName = eggDirectoryInfo.Parent.Name + "/" + eggDirectoryInfo.Name;
                  hatchlingSpawner.Spawn(
                     eggFullName,
                     new SpawnConfiguration {
                        InstanceName = bootstrapDto.Name,
                        Arguments = bootstrapDto.PayloadBytes,
                        StartFlags = HatchlingStartFlags.StartAsynchronously
                     });
               }
            } catch (Exception e) {
               logger.Error("Threw processing restart signal " + signalPath, e);
            } finally {
               File.Delete(signalPath);
            }
         }
      }
   }
}
