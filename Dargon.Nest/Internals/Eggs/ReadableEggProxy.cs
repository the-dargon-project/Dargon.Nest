using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Eggs {
   public class ReadableEggProxy : ReadableEgg {
      public ReadableEggProxy(ReadableEggMetadata metadata, ReadableEggRepository repository) {
         Metadata = metadata;
         Repository = repository;
      }

      // ReadableEgg
      public ReadableEggMetadata Metadata { get; }
      public ReadableEggRepository Repository { get; }

      // ReadableEggMetadata
      public string Name => Metadata.Name;
      public string Version => Metadata.Version;
      public string Remote => Metadata.Remote;

      // ReadableEggRepository
      public string Location => Repository.Location;
      public Task<IEnumerable<EggFileEntry>> EnumerateFilesAsync() => Repository.EnumerateFilesAsync();
      public string ComputeFullPath(string internalPath) => Repository.ComputeFullPath(internalPath);
   }
}