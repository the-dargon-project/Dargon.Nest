using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItzWarty.Collections;
using ItzWarty.IO;
using IOPath = System.IO.Path;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface NestContext {
      string Name { get; }
      string Path { get; }
      string Version { get; }
      string Remote { get; }

      EggContext GetEggByName(string eggName);
      bool TryGetEggByName(string eggName, out EggContext eggContext);
   }

   public class NestContextImpl : NestContext {
      private readonly object synchronization = new object();
      private readonly ConcurrentDictionary<string, EggContext> eggsByName = new ConcurrentDictionary<string, EggContext>();
      private readonly string nestName;
      private readonly string nestPath;
      private readonly LocalDargonNest nest;

      public NestContextImpl(string nestName, string nestPath, LocalDargonNest nest) {
         this.nestName = nestName;
         this.nestPath = nestPath;
         this.nest = nest;
      }

      public string Name => nestName;
      public string Path => nestPath;
      public string Version => TryReadOrNull(IOPath.Combine(Path, "VERSION"));
      public string Remote => TryReadOrNull(IOPath.Combine(Path, "REMOTE"));

      public EggContext GetEggByName(string eggName) {
         EggContext result;
         if (!TryGetEggByName(eggName, out result)) {
            throw new KeyNotFoundException($"Unable to find egg of name {eggName} in nest {this.nestName}");
         }
         return result;
      }

      public bool TryGetEggByName(string eggName, out EggContext eggContext) {
         if (eggsByName.TryGetValue(eggName, out eggContext)) {
            return true;
         }
         lock (synchronization) {
            if (eggsByName.TryGetValue(eggName, out eggContext)) {
               return true;
            }
            var egg = (LocalDargonEgg)nest.EnumerateEggs().FirstOrDefault(x => x.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
            if (egg == null) {
               eggContext = null;
               return false;
            } else {
               eggContext = eggsByName[eggName] = new EggContext(egg, this);
               return true;
            }
         }
      }

      private string TryReadOrNull(string path) {
         if (!File.Exists(path)) {
            return null;
         } else {
            return File.ReadAllText(path);
         }
      }
   }
}
