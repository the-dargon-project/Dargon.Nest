using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Daemon.Init;
using Dargon.Nest.Daemon.Init.Handlers;
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
         Singleton<NestServiceImpl>();
         Singleton<HatchlingDirectoryImpl>();
         Singleton<ReadableHatchlingDirectory, HatchlingDirectoryImpl>();
         Singleton<ManageableHatchlingDirectory, HatchlingDirectoryImpl>();
         Singleton<NestDirectoryImpl>();
         Singleton<UpdateFetcher, UpdateFetcherImpl>();
         Singleton<StagedUpdateProcessor, StagedUpdateProcessorImpl>();
         Singleton<StagedUpdateProcessorImpl>(ConstructStagedUpdateProcessor);
         Singleton<ReadableNestDirectory, NestDirectoryImpl>();
         Singleton<ManageableNestDirectory, NestDirectoryImpl>();
         Singleton<NestDirectorySynchronizer>(RyuTypeFlags.Required);
         Singleton<NestLockManager>();
         Singleton<RunEggInitScriptActionHandlerImpl>(RyuTypeFlags.Required);
         Singleton<InitScriptRunner>(ConstructInitScriptRunner);
         Singleton<NestDaemonServiceImpl>();
         LocalService<ExeggutorService, NestServiceImpl>();
         LocalService<NestDaemonService, NestDaemonServiceImpl>();
         PofContext<ExeggutorHostPofContext>();
         Mob<ExeggutorMob>();
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
            ryu.Get<ReadableNestDirectory>(),
            ryu.Find<InitScriptActionHandler>().ToDictionary(
               handler => handler.ActionName));
      }
   }
}
