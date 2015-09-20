using System;
using System.Collections.Generic;
using System.Linq;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface ReadableHatchlingDirectory {
      IReadOnlySet<HatchlingContext> EnumerateHatchlings();
      IEnumerable<HatchlingContext> EnumerateHatchlingsOfNest(string nestName);
   }

   public interface ManageableHatchlingDirectory : ReadableHatchlingDirectory {
      void RegisterHatchling(HatchlingContext hatchling);
   }

   public class HatchlingDirectoryImpl : ManageableHatchlingDirectory {
      private readonly IConcurrentSet<HatchlingContext> hatchlings = new ConcurrentSet<HatchlingContext>();

      public void RegisterHatchling(HatchlingContext hatchling) {
         hatchlings.Add(hatchling);
      }

      public IReadOnlySet<HatchlingContext> EnumerateHatchlings() {
         return hatchlings;
      }

      public IEnumerable<HatchlingContext> EnumerateHatchlingsOfNest(string nestName) {
         return hatchlings.Where(h => h.Nest.Name.Equals(nestName, StringComparison.OrdinalIgnoreCase));
      }
   }
}