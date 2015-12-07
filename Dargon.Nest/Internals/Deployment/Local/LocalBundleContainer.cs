using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Deployment.Local {
   public class LocalBundleContainer : ManageableBundleContainer {
      private readonly string path;
      private readonly string bundlesDirectoryPath;

      public LocalBundleContainer(string path) {
         this.path = path;
         this.bundlesDirectoryPath = Path.Combine(path, NestConstants.kBundlesDirectoryName);
      }

      public string Location => path;

      public Task<IEnumerable<ManageableBundle>> EnumerateBundlesAsync() => Task.FromResult(EnumerateBundlesHelper());

      private IEnumerable<ManageableBundle> EnumerateBundlesHelper() {
         IoUtilities.PrepareDirectory(bundlesDirectoryPath);
         foreach (var bundlePath in Directory.EnumerateDirectories(bundlesDirectoryPath)) {
            yield return BundleFactory.Local(bundlePath);
         }
      }

      async Task<IEnumerable<ReadableBundle>> ReadableBundleContainer.EnumerateBundlesAsync() => await EnumerateBundlesAsync();
   }
}