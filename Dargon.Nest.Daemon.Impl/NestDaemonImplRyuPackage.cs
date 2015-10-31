using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.Ryu;

namespace Dargon.Nest.Daemon {
   public class NestDaemonImplRyuPackage : RyuPackageV1 {
      public NestDaemonImplRyuPackage() {
         Singleton<InternalNestDaemonService, InternalNestDaemonServiceImpl>();
         Singleton<HatchlingSpawner, HatchlingSpawnerImpl>();
         Singleton<HostOperations>();
         Singleton<HostProcessFactory>();
         Singleton<NestServiceImpl>();
         Singleton<HatchlingDirectoryImpl>();
         Singleton<ReadableHatchlingDirectory, HatchlingDirectoryImpl>();
         Singleton<ManageableHatchlingDirectory, HatchlingDirectoryImpl>();
         Singleton<NestDirectory>();
         Singleton<NestDirectorySynchronizer>(RyuTypeFlags.Required);
         Singleton<NestLockManager>();
         LocalService<ExeggutorService, NestServiceImpl>();
         PofContext<ExeggutorPofContext>();
         PofContext<ExeggutorHostPofContext>();
         Mob<ExeggutorMob>();
      }
   }
}
