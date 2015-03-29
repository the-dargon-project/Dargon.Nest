using System;
using System.Net.Sockets;
using Dargon.Nest.Eggxecutor;
using ItzWarty.Collections;
using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public class ExeggutorServiceImpl : ExeggutorService {
      private readonly EggContextFactory eggContextFactory;
      private readonly IConcurrentDictionary<string, IEggContext> eggContextsByName;

      public ExeggutorServiceImpl(
         EggContextFactory eggContextFactory
      ) : this(
         eggContextFactory, 
         new ConcurrentDictionary<string, IEggContext>()
      ){}

      public ExeggutorServiceImpl(EggContextFactory eggContextFactory, IConcurrentDictionary<string, IEggContext> eggContextsByName) {
         this.eggContextFactory = eggContextFactory;
         this.eggContextsByName = eggContextsByName;
      }

      public void LoadNest(string nestPath) {
         var nest = new LocalDargonNest(nestPath);
         foreach (var egg in nest.EnumerateEggs()) {
            eggContextsByName.Add(egg.Name, eggContextFactory.Create(egg));
         }
      }
   }

   public interface IEggContext {}

   public class EggContext : IEggContext {
      private readonly RemoteHostContextFactory remoteHostContextFactory;
      private readonly IProcessProxy processProxy;
      private readonly IDargonEgg egg;

      public EggContext(RemoteHostContextFactory remoteHostContextFactory, IProcessProxy processProxy, IDargonEgg egg) {
         this.remoteHostContextFactory = remoteHostContextFactory;
         this.processProxy = processProxy;
         this.egg = egg;
      }

      public void Spawn(string name) {
         remoteHostContextFactory.Create(name, egg.Location);
      }
   }

   public class EggInstance {
   }
}
