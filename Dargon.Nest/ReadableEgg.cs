using Dargon.Nest.Internals.Eggs;

namespace Dargon.Nest {
   public interface ReadableEgg : ReadableEggMetadata, ReadableEggRepository {
      ReadableEggMetadata Metadata { get; }
      ReadableEggRepository Repository { get; }
   }
}
