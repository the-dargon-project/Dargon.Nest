using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Bundles.Remote {
   public class RemoteUrlBackedBundleMetadata : ReadableBundleMetadata {
      private readonly string location;
      private readonly Lazy<string> initScriptContents; 

      public RemoteUrlBackedBundleMetadata(string name, string version, string remote, string location) {
         this.location = location;
         Name = name;
         Version = version;
         Remote = remote;
         initScriptContents = new Lazy<string>(LoadInitScript);
      }

      private string LoadInitScript() {
         return IoUtilities.ReadStringOrFallbackAsync(IoUtilities.CombinePath(location, NestConstants.kInitJsonFileName)).Result;
      }

      public string Name { get; }
      public string Version { get; }
      public string Remote { get; }
      public string InitScript => initScriptContents.Value;
   }

   public class RemoteUrlBackedEggContainer : ReadableEggContainer {
      public RemoteUrlBackedEggContainer(string name, string version, string remote, string location) {
         Name = name;
         Version = version;
         Remote = remote;
         Location = location;
      }

      public string Name { get; }
      public string Version { get; }
      public string Remote { get; }
      public string Location { get; }

      public async Task<IEnumerable<ReadableEgg>> EnumerateEggsAsync() {
         var contentsPath = IoUtilities.CombinePath(Location, NestConstants.kEggsFileName);
         var contentsString = await IoUtilities.ReadStringAsync(contentsPath);
         return from line in contentsString.Split('\n')
                let trimmedLine = line.Trim()
                let parts = trimmedLine.Split(' ')
                let eggName = parts[0]
                let eggVersion = parts[1]
                select EggFactory.RemoteByVersion(Remote, eggName, eggVersion);
      }
   }
}
