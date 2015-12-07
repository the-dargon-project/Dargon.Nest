using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Eggs;
using Dargon.Nest.Internals.Eggs.InMemory;
using Dargon.Nest.Internals.Eggs.Local;
using Dargon.Nest.Internals.Eggs.Remote;

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

      public static ManageableEgg Local(string location) {
         var repository = new LocalEggRepository(location);
         return new ManageableEggProxy(
            new LocalEggMetadata(repository), repository);
      }

      private static ReadableEgg RemoteByVersion(string location) {
         location = location.Replace('\\', '/');
         if (!location.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
            throw new NotSupportedException("Expected url to start with http but found " + location);
         }
         var remote = location.Substring(0, location.IndexOf('/', "http://".Length));
         var remainder = location.Substring(remote.Length);
         var parts = remainder.Split('/');
         var i = 0;
         while (parts[i] != NestConstants.kEggsDirectoryName) i++;
         i++; // skip over eggs directory
         var eggName = parts[i];
         i++; // skip over egg name;
         i++; // skip over releases name
         var releaseName = parts[i];
         var releaseVersion = releaseName.Substring(eggName.Length + 1);
         return RemoteByVersion(remote, eggName, releaseVersion);
      }

      public static ReadableEgg RemoteByVersion(string remote, string name, string version) {
         return new ReadableEggProxy(
            new RemoteEggMetadata(name, version, remote),
            new RemoteEggRepository(remote, name, version));
      }

      public static async Task<ReadableEggRepository> RemoteByChannelLatestAsync(string remote, string name, string channel) {
         var channelUrl = IoUtilities.CombinePath(remote, NestConstants.kEggsDirectoryName, name, NestConstants.kChannelsDirectoryName, channel);
         var latestVersionString = await IoUtilities.ReadStringAsync(channelUrl);
         return RemoteByVersion(remote, name, latestVersionString);
      }

      public static ReadableEgg FromPath(string location) {
         if (IoUtilities.IsLocal(location)) {
            return Local(location);
         } else {
            return RemoteByVersion(location);
         }
      }
   }
}
