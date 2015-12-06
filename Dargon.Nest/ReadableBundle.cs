using System.Collections.Generic;
using Dargon.Nest.Internals.Bundles;

namespace Dargon.Nest {
   public interface ReadableBundle : ReadableBundleMetadata, ReadableEggContainer {
      ReadableBundleMetadata Metadata { get; }
      ReadableEggContainer EggContainer { get; }
   }
}