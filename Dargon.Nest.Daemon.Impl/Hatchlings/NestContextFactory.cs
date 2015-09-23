using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Hatchlings {
   public class NestContextFactory {
      private readonly IFileSystemProxy fileSystemProxy;

      public NestContextFactory(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public NestContext Create(string path) {
         var directoryInfo = fileSystemProxy.GetDirectoryInfo(path);
         return new NestContextImpl(directoryInfo.Name, new LocalDargonNest(directoryInfo.FullName));
      }
   }
}
