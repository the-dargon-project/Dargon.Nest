using Dargon.Nest.Internals.Bundles;
using Fody.Constructors;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Hatchlings {
   [RequiredFieldsConstructor]
   public class BundleContextFactory {
      private readonly IFileSystemProxy fileSystemProxy = null;

      public BundleContext Create(string path) {
         var directoryInfo = fileSystemProxy.GetDirectoryInfo(path);
         return new BundleContextImpl(directoryInfo.Name, directoryInfo.FullName, BundleFactory.Local(directoryInfo.FullName));
      }
   }
}
