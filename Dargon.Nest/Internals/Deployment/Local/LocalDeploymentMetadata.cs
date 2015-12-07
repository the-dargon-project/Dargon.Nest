using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Deployment.Local {
   public class LocalDeploymentMetadata : ReadableDeploymentMetadata {
      private readonly string path;

      public LocalDeploymentMetadata(string path) {
         this.path = path;
      }

      public string Name => IoUtilities.ExtractNameFromPath(path);
      public string Version => IoUtilities.ReadStringAsync(IoUtilities.CombinePath(path, NestConstants.kVersionFileName)).Result;
      public string Remote => IoUtilities.ReadStringAsync(IoUtilities.CombinePath(path, NestConstants.kRemoteFileName)).Result;
      public string Channel => IoUtilities.ReadStringAsync(IoUtilities.CombinePath(path, NestConstants.kChannelFileName)).Result;
   }
}
