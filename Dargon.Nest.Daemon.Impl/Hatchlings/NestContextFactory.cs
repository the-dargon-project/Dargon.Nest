using Fody.Constructors;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Hatchlings {
   [RequiredFieldsConstructor]
   public class NestContextFactory {
      private readonly IFileSystemProxy fileSystemProxy = null;

      public NestContext Create(string path) {
         var directoryInfo = fileSystemProxy.GetDirectoryInfo(path);
         return new NestContextImpl(directoryInfo.Name, directoryInfo.FullName, new LocalDargonNest(directoryInfo.FullName));
      }
   }
}
