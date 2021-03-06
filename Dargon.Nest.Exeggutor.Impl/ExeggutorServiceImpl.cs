﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Dargon.Nest.Egg;
using Dargon.Nest.Eggxecutor;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using NLog;

namespace Dargon.Nest.Exeggutor {
   public class ExeggutorServiceImpl : ExeggutorService {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly object synchronization = new object();
      private readonly string nestPath;
      private readonly EggContextFactory eggContextFactory;
      private readonly IConcurrentDictionary<Guid, HatchlingContext> hatchlingContextsById;
      private readonly IConcurrentDictionary<string, HatchlingContext> hatchlingContextsByName;
      private bool isShuttingDown = false;

      public ExeggutorServiceImpl(
         string nestPath,
         EggContextFactory eggContextFactory
      ) : this(
         nestPath,
         eggContextFactory,
         new ConcurrentDictionary<Guid, HatchlingContext>(),
         new ConcurrentDictionary<string, HatchlingContext>()
      ) { }

      public ExeggutorServiceImpl(
         string nestPath,
         EggContextFactory eggContextFactory,
         IConcurrentDictionary<Guid, HatchlingContext> hatchlingContextsById,
         IConcurrentDictionary<string, HatchlingContext> hatchlingContextsByName
      ) {
         this.nestPath = nestPath;
         this.eggContextFactory = eggContextFactory;
         this.hatchlingContextsById = hatchlingContextsById;
         this.hatchlingContextsByName = hatchlingContextsByName;
      }

      public IEnumerable<HatchlingContext> Hatchlings => hatchlingContextsById.Values;

      public SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration) {
         try {
            logger.Info("Spawning hatchling {0}!", eggName);
            configuration = configuration ?? new SpawnConfiguration();
            configuration.Arguments = configuration.Arguments ?? new byte[0];
            configuration.InstanceName = configuration.InstanceName ?? eggName;

            lock (synchronization) {
               if (isShuttingDown) {
                  return new SpawnHatchlingResult { StartResult = NestResult.Failure };
               }

               IEggContext eggContext;
               if (!TryCreateEggContext(eggName, out eggContext)) {
                  throw new EggNotFoundException(eggName);
               } else {
                  var hatchlingContext = eggContext.Spawn(configuration);
                  hatchlingContextsById.Add(hatchlingContext.InstanceId, hatchlingContext);
                  if (hatchlingContext.Name != null) {
                     hatchlingContextsByName.Add(hatchlingContext.Name, hatchlingContext);
                  }
                  hatchlingContext.Exited += (s, e) => {
                     logger.Info("Hatchling " + hatchlingContext.Name + " has exited!");
                     hatchlingContextsByName.Remove(hatchlingContext.Name.PairValue(hatchlingContext));
                     hatchlingContextsById.Remove(hatchlingContext.InstanceId.PairValue(hatchlingContext));
                  };

                  NestResult startResult = NestResult.Unknown;
                  if (!configuration.StartFlags.HasFlag(HatchlingStartFlags.StartAsynchronously)) {
                     logger.Info("Getting start result");
                     startResult = hatchlingContext.StartResult;
                     logger.Info("got start result");
                  }

                  logger.Info("Returning from SpawnHatchling");

                  return new SpawnHatchlingResult {
                     HatchlingId = hatchlingContext.InstanceId,
                     StartResult = startResult
                  };
               }
            }
         } catch (Exception e) {
            logger.Error("SpawnHatchling threw", e);
            throw;
         }
      }

      public void KillAllHatchlings() {
         lock (synchronization) {
            var hatchlings = Hatchlings.ToArray();
            var exitCountdown = new CountdownEvent(hatchlings.Length);
            foreach (var hatchling in hatchlings) {
               hatchling.Exited += (s, e) => exitCountdown.Signal();
               hatchling.Shutdown();
            }
            exitCountdown.Wait();
         }
      }

      public void KillAllHatchlingsAndUpdateAllPackages() {
         lock (synchronization) {
            KillAllHatchlings();
            var nest = new LocalDargonNest(nestPath);
            nest.UpdateNest();
         }
      }

      public void KillAllHatchlingsAndPrepareForShutdown() {
         lock (synchronization) {
            KillAllHatchlings();
            isShuttingDown = true;
         }
      }

      private bool TryCreateEggContext(string eggName, out IEggContext eggContext) {
         var nest = new LocalDargonNest(nestPath);
         var egg = nest.EnumerateEggs().FirstOrDefault(x => x.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
         if (egg != null) {
            eggContext = eggContextFactory.Create(egg);
            return true;
         } else {
            eggContext = null;
            return false;
         }
      }
   }
}
