using System.Collections.Generic;

namespace Dargon.Nest {
   public interface IDargonNest {
      string Channel { get; set; }
      string Remote { get; set; }
      IEnumerable<IDargonEgg> EnumerateEggs();
   }
}