using System.Collections.Generic;

namespace Dargon.Nest.Internals.Eggs {
   public class ManageableEggProxy : ManageableEgg {
      public ManageableEggProxy(ReadableEggMetadata metadata, ManageableEggRepository repository) {
         Metadata = metadata;
         Repository = repository;
      }

      // ReadableEgg
      public ReadableEggMetadata Metadata { get; }
      ReadableEggRepository ReadableEgg.Repository => Repository;

      // ReadableEggMetadata
      public string Name => Metadata.Name;
      public string Version => Metadata.Version;
      public string Remote => Metadata.Remote;

      // ManageableEgg
      public ManageableEggRepository Repository { get; }
      public IEnumerable<EggFileEntry> EnumerateFiles() => Repository.EnumerateFiles();
      public string ComputeFullPath(string internalPath) => Repository.ComputeFullPath(internalPath);

      // ManageableEggRepository
      public string Location => Repository.Location;
      public void Sync(ReadableEggRepository remote) => Repository.Sync(remote);
   }
}
