namespace Dargon.Nest.Internals.Eggs.Local {
   public class LocalEggMetadata : ReadableEggMetadata {
      private readonly LocalEggRepository repository;

      public LocalEggMetadata(LocalEggRepository repository) {
         this.repository = repository;
      }

      public string Name => IoUtilities.ExtractNameFromPath(repository.Location);
      public string Version => IoUtilities.ReadStringOrFallbackAsync(repository.ComputeFullPath("version")).Result;
      public string Remote => IoUtilities.ReadStringOrFallbackAsync(repository.ComputeFullPath("remote")).Result;
   }
}
