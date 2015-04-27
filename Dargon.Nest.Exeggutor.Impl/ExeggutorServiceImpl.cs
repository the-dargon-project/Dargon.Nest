using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using Dargon.Nest.Eggxecutor;
using ItzWarty;
using ItzWarty.Collections;
using NLog;

namespace Dargon.Nest.Exeggutor {
   public class ExeggutorServiceImpl : ExeggutorService {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly string nestPath;
      private readonly EggContextFactory eggContextFactory;
      private readonly IConcurrentDictionary<Guid, HatchlingContext> hatchlingContextsById;
      private readonly IConcurrentDictionary<string, HatchlingContext> hatchlingContextsByName;

      public ExeggutorServiceImpl(
         string nestPath,
         EggContextFactory eggContextFactory
      ) : this(
         nestPath,
         eggContextFactory,
         new ConcurrentDictionary<Guid, HatchlingContext>(),
         new ConcurrentDictionary<string, HatchlingContext>()
      ) {}

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

      public Guid SpawnHatchling(string eggName, SpawnConfiguration configuration) {
         try {
            Console.WriteLine("Spawning hatchling {0}!", eggName);
            configuration = configuration ?? new SpawnConfiguration();

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
                  Console.WriteLine("Hatchling " + hatchlingContext.Name + " has exited!");
                  hatchlingContextsByName.Remove(hatchlingContext.Name.PairValue(hatchlingContext));
               };
               return hatchlingContext.InstanceId;
            }
         } catch (Exception e) {
            logger.Error("SpawnHatchling threw", e);
            return Guid.Empty;
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
