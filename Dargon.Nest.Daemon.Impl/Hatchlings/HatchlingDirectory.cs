using ItzWarty.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface ReadableHatchlingDirectory {
      bool TryGetHatchlingByName(string name, out HatchlingContext hatchling);
      IReadOnlySet <HatchlingContext> EnumerateHatchlings();
      IEnumerable<HatchlingContext> EnumerateHatchlingsOfNest(string nestName);
   }

   public interface ManageableHatchlingDirectory : ReadableHatchlingDirectory {
      void RegisterHatchling(HatchlingContext hatchling);
   }

   public class HatchlingDirectoryImpl : ManageableHatchlingDirectory {
      private readonly IConcurrentDictionary<string, HatchlingContext> hatchlingsByName = new ConcurrentDictionary<string, HatchlingContext>();
      private readonly IConcurrentSet<HatchlingContext> hatchlings = new ConcurrentSet<HatchlingContext>();

      public void RegisterHatchling(HatchlingContext hatchling) {
         hatchlings.Add(hatchling);
         hatchlingsByName.Add(hatchling.Name, hatchling);
      }

      public bool TryGetHatchlingByName(string name, out HatchlingContext hatchling) => hatchlingsByName.TryGetValue(name, out hatchling);

      public IReadOnlySet<HatchlingContext> EnumerateHatchlings() {
         return hatchlings;
      }

      public IEnumerable<HatchlingContext> EnumerateHatchlingsOfNest(string nestName) {
         return hatchlings.Where(h => h.Nest.Name.Equals(nestName, StringComparison.OrdinalIgnoreCase));
      }
   }
}