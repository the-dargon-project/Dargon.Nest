using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Dargon.Nest.Internals.Eggs.Remote {
   public class RemoteEggMetadata : ReadableEggMetadata {
      public RemoteEggMetadata(string name, string version, string remote) {
         Name = name;
         Version = version;
         Remote = remote;
      }

      public string Name { get; }
      public string Version { get; }
      public string Remote { get; }
   }

   public class RemoteEggRepository : ManageableEggRepository {
      private readonly AsyncLock synchronization = new AsyncLock();
      private readonly string remote;
      private readonly string name;
      private readonly string version;
      private readonly string cacheLocation;
      private readonly string location;
      private IReadOnlyList<EggFileEntry> fileEntries = null;

      public RemoteEggRepository(string remote, string name, string version) {
         this.remote = remote;
         this.name = name;
         this.version = version;
         this.cacheLocation = IoUtilities.CombinePath(remote, NestConstants.kEggsDirectoryName, name, NestConstants.kCacheDirectoryName);
         this.location = IoUtilities.CombinePath(remote, NestConstants.kEggsDirectoryName, name, NestConstants.kReleasesDirectoryName, $"{name}-{version}");
      }

      public string Location => location;

      public async Task<IEnumerable<EggFileEntry>> EnumerateFilesAsync() {
         if (fileEntries != null) {
            return fileEntries;
         }

         using (await synchronization.LockAsync()) {
            Thread.MemoryBarrier();
            if (fileEntries == null) {
               var data = await IoUtilities.ReadStringAsync(IoUtilities.CombinePath(location, NestConstants.kFileListFileName));
               fileEntries = EggFileEntrySerializer.Deserialize(data);
            }
            return fileEntries;
         }
      }

      public string ComputeFullPath(string internalPath) {
         foreach (var file in EnumerateFilesAsync().Result) {
            if (file.InternalPath.Equals(internalPath, StringComparison.OrdinalIgnoreCase)) {
               return IoUtilities.CombinePath(cacheLocation, file.Guid.ToString("n"));
            }
         }
         throw new KeyNotFoundException("Could not find match for internal path " + internalPath);
      }

      public Task SyncAsync(ReadableEggRepository remote) {
         throw new NotSupportedException("Cannot sync remote repository.");
      }
   }
}