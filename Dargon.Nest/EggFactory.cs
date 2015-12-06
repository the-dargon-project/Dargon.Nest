using System.Collections.Generic;
using System.IO;
using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Eggs;
using Dargon.Nest.Internals.Eggs.Common;
using Dargon.Nest.Internals.Eggs.InMemory;

namespace Dargon.Nest {
   public static class EggFactory {
      public static ReadableEgg InMemory(string name, string location, string version) {
         location = IoUtilities.FormatSystemPath(location);

         var entries = new List<EggFileEntry>();
         foreach (var path in Directory.EnumerateFiles(location, "*", SearchOption.AllDirectories)) {
            var hash = IoUtilities.ComputeLocalFileHash(path);
            var internalPath = IoUtilities.GetDescendentRelativePath(location, path);
            var entry = new EggFileEntry(hash, internalPath);
            entries.Add(entry);
         }

         return new ManageableEggProxy(new InMemoryEggMetadata(name, version), new InMemoryEggRepository(location, entries));
      }

      public static ManageableEgg Local(string location) => FileBacked(location);

      public static ReadableEgg Remote(string address) => FileBacked(address);

      public static ManageableEgg FileBacked(string path) {
         var repository = new LocationBackedEggRepository(IoUtilities.FormatSystemPath(path));
         return new ManageableEggProxy(new RepositoryBackedEggMetadata(repository), repository);
      }
   }
}
