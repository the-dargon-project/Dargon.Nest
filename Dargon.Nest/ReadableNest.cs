using System.Collections.Generic;
using System.IO;

namespace Dargon.Nest {
   public interface ReadableNest {
      IEnumerable<ReadableBundle> EnumerateBundles();
   }
}