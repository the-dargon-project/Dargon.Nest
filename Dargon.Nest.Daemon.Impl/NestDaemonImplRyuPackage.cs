using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Daemon.Init;
using Dargon.Nest.Daemon.Init.Handlers;
using Dargon.Nest.Daemon.Management;
using Dargon.Nest.Daemon.Restart;
using Dargon.Nest.Daemon.Updating;
using Dargon.Nest.Daemon.Utilities;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.Ryu;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon {
   public class NestDaemonImplRyuPackage : RyuPackageV1 {
      public NestDaemonImplRyuPackage() {
         Singleton<FileUtilities, FileUtilitiesImpl>();
         Singleton<HatchlingSpawner, HatchlingSpawnerImpl>();
         Singleton<HostOperations>();
         Singleton<HostProcessFactory>();
         Singleton<ExeggutorServiceDispatchProxyImpl>();
         Singleton<HatchlingDirectoryImpl>();
         Singleton<ReadableHatchlingDirectory, HatchlingDirectoryImpl>();
         Singleton<ManageableHatchlingDirectory, HatchlingDirectoryImpl>();
         Singleton<BundleDirectoryImpl>();
         Singleton<StagedUpdateProcessor, StagedUpdateProcessorImpl>();
         Singleton<StagedUpdateProcessorImpl>(ConstructStagedUpdateProcessor);
         Singleton<ReadableBundleDirectory, BundleDirectoryImpl>();
         Singleton<ManageableBundleDirectory, BundleDirectoryImpl>();
         Singleton<NestDirectorySynchronizer>(RyuTypeFlags.Required);
         Singleton<NestLockManager>();
         Singleton<RunEggInitScriptActionHandlerImpl>(RyuTypeFlags.Required);
         Singleton<InitScriptRunner>(ConstructInitScriptRunner);
         Singleton<NestDaemonServiceImpl>();
         LocalService<ExeggutorService, ExeggutorServiceDispatchProxyImpl>();
         Singleton<ReadableDeployment, ManageableDeployment>();
         Singleton<ManageableDeployment>(GetLocalDeployment);
         Singleton<HatchlingSpawnerServiceImpl>();
         Singleton<HatchlingDirectoryServiceImpl>();
         Singleton<HatchlingKillerServiceImpl>();
         Singleton<HatchlingPatcherServiceImpl>();
         Singleton<HatchlingPatcherWorkerImpl>();
         LocalService<NestDaemonService, NestDaemonServiceImpl>();
         PofContext<ExeggutorHostPofContext>();
         Mob<ExeggutorMob>();

         Singleton<RestartSignalService>();
         Mob<RestartMob>();
      }

      private ManageableDeployment GetLocalDeployment(RyuContainer ryu) {
         var assemblyPath = Assembly.GetExecutingAssembly().Location;
         var deploymentPath = Path.Combine(assemblyPath, "..", "..", "..", "..");
         return DeploymentFactory.Local(deploymentPath);
      }

      private StagedUpdateProcessor ConstructStagedUpdateProcessor(RyuContainer ryu) {
         var fileSystemProxy = ryu.Get<IFileSystemProxy>();
         var configuration = ryu.Get<DaemonConfiguration>();
         var stageDirectoryInfo = fileSystemProxy.GetDirectoryInfo(configuration.StagePath);
         var nestsDirectoryInfo = fileSystemProxy.GetDirectoryInfo(configuration.NestsPath);
         return new StagedUpdateProcessorImpl(
            ryu.Get<FileUtilities>(),
            stageDirectoryInfo,
            nestsDirectoryInfo);
      }

      private InitScriptRunner ConstructInitScriptRunner(RyuContainer ryu) {
         return new InitScriptRunnerImpl(
            ryu.Get<ReadableBundleDirectory>(),
            ryu.Find<InitScriptActionHandler>().ToDictionary(
               handler => handler.ActionName));
      }
   }
}
