using System.Collections.Generic;

namespace Dargon.Nest.Internals.Eggs {
   public interface ReadableEggRepository {
      string Location { get; }
      IEnumerable<EggFileEntry> EnumerateFiles();
      string ComputeFullPath(string internalPath);
   }
}