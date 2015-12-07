using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Eggs {
   public interface ReadableEggRepository {
      string Location { get; }
      Task<IEnumerable<EggFileEntry>> EnumerateFilesAsync();
      string ComputeFullPath(string internalPath);
   }
}