using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Deployment {
   public interface ReadableBundleContainer {
      string Location { get; }
      Task<IEnumerable<ReadableBundle>> EnumerateBundlesAsync();
   }
   public interface ManageableBundleContainer : ReadableBundleContainer {
      new Task<IEnumerable<ManageableBundle>> EnumerateBundlesAsync();
   }
}