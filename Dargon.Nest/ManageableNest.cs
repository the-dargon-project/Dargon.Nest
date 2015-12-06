using System.Collections.Generic;

namespace Dargon.Nest {
   public interface ManageableNest : ReadableNest {
      new IEnumerable<ManageableBundle> EnumerateBundles();
   }
}