﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Eggxecutor;
using Dargon.Nest.Exeggutor;
using NLog;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface HatchlingSpawner {
      HatchlingContext Spawn(string eggName, SpawnConfiguration spawnConfiguration);
   }

   public class HatchlingSpawnerImpl : HatchlingSpawner {
      private static Logger logger = LogManager.GetCurrentClassLogger();
      private readonly DaemonConfiguration daemonConfiguration;
      private readonly HostOperations hostOperations;
      private readonly HostProcessFactory hostProcessFactory;
      private readonly EggDirectory eggDirectory;
      private readonly ManageableHatchlingDirectory hatchlingDirectory;

      public HatchlingSpawnerImpl(DaemonConfiguration daemonConfiguration, HostOperations hostOperations, HostProcessFactory hostProcessFactory, EggDirectory eggDirectory, ManageableHatchlingDirectory hatchlingDirectory) {
         this.daemonConfiguration = daemonConfiguration;
         this.hostOperations = hostOperations;
         this.hostProcessFactory = hostProcessFactory;
         this.eggDirectory = eggDirectory;
         this.hatchlingDirectory = hatchlingDirectory;
      }

      public HatchlingContext Spawn(string eggName, SpawnConfiguration spawnConfiguration) {
         Guid hatchlingId = Guid.NewGuid();

         logger.Info($"Spawning instance of egg {eggName} with id {hatchlingId}!");
         spawnConfiguration = spawnConfiguration ?? new SpawnConfiguration();
         spawnConfiguration.Arguments = spawnConfiguration.Arguments ?? new byte[0];
         spawnConfiguration.InstanceName = spawnConfiguration.InstanceName ?? hatchlingId.ToString("n");

         var eggContext = eggDirectory.GetContextByName(eggName);
         hostOperations.CopyHostToLocalEgg(eggContext.Egg);
         var hostProcess = hostProcessFactory.CreateAndInitialize(eggContext, spawnConfiguration);
         return new HatchlingContextImpl(hatchlingId, hostProcess, spawnConfiguration, eggContext, hatchlingDirectory);
      }
   }
}