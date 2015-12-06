namespace Dargon.Nest.Internals.Eggs.Common {
   public class RepositoryBackedEggMetadata : ReadableEggMetadata {
      private readonly ReadableEggRepository repository;

      public RepositoryBackedEggMetadata(ReadableEggRepository repository) {
         this.repository = repository;
      }

      public string Name => IoUtilities.ExtractNameFromPath(repository.Location);
      public string Version => IoUtilities.ReadString(repository.ComputeFullPath("version"));
      public string Remote => IoUtilities.ReadString(repository.ComputeFullPath("remote"));
   }
}
