using ItzWarty;
using ItzWarty.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface ReadableHatchlingDirectory {
      bool TryGetHatchlingByName(string name, out HatchlingContext hatchling);
      IReadOnlySet<HatchlingContext> EnumerateHatchlings();
      IEnumerable<HatchlingContext> EnumerateHatchlingsOfBundle(string nestName);
   }

   public interface ManageableHatchlingDirectory : ReadableHatchlingDirectory {
      void RegisterHatchling(HatchlingContext hatchling);
      void UnregisterHatchling(HatchlingContext hatchling);
   }

   public class HatchlingDirectoryImpl : ManageableHatchlingDirectory {
      private readonly IConcurrentDictionary<string, HatchlingContext> hatchlingsByName = new ConcurrentDictionary<string, HatchlingContext>();
      private readonly IConcurrentSet<HatchlingContext> hatchlings = new ConcurrentSet<HatchlingContext>();

      public void RegisterHatchling(HatchlingContext hatchling) {
         hatchlings.Add(hatchling);
         hatchlingsByName.Add(hatchling.Name, hatchling);
      }

      public void UnregisterHatchling(HatchlingContext hatchling) {
         hatchlings.Remove(hatchling);
         hatchlingsByName.TryRemove(hatchling.Name, hatchling);
      }

      public bool TryGetHatchlingByName(string name, out HatchlingContext hatchling) => hatchlingsByName.TryGetValue(name, out hatchling);

      public IReadOnlySet<HatchlingContext> EnumerateHatchlings() {
         return hatchlings;
      }

      public IEnumerable<HatchlingContext> EnumerateHatchlingsOfBundle(string nestName) {
         return hatchlings.Where(h => h.Bundle.Name.Equals(nestName, StringComparison.OrdinalIgnoreCase));
      }
   }
}