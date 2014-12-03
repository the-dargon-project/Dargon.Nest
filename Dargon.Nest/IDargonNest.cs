using System.Collections.Generic;

namespace Dargon.Nest {
   public interface IDargonNest {
      IEnumerable<IDargonEgg> EnumerateEggs();
   }
}