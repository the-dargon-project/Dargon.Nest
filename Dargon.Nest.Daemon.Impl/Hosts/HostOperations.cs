using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Hosts {
   public class HostOperations {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly DaemonConfiguration daemonConfiguration;

      public HostOperations(IFileSystemProxy fileSystemProxy, DaemonConfiguration daemonConfiguration) {
         this.fileSystemProxy = fileSystemProxy;
         this.daemonConfiguration = daemonConfiguration;
      }
      
      public void CopyHostToLocalEgg(LocalDargonEgg localEgg) {
         var nestHostFileInfo = fileSystemProxy.GetFileInfo(daemonConfiguration.HostExecutablePath);
         var nestHostDirectory = nestHostFileInfo.Parent;
         var nestHostAssemblies = from file in nestHostDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                                  where file.Name.EndsWithAny(new[] { ".exe", ".dll" }, StringComparison.OrdinalIgnoreCase)
                                  select file;

         foreach (var assemblyFileInfo in nestHostAssemblies) {
            try {
               var relativePath = assemblyFileInfo.FullName.Substring(nestHostDirectory.FullName.Length + 1);
               var destinationPath = Path.Combine(localEgg.RootPath, relativePath);

               if (localEgg.Files.None(x => x.InternalPath.Equals(relativePath, StringComparison.OrdinalIgnoreCase))) {
                  File.Copy(assemblyFileInfo.FullName, destinationPath, true);
               }
            } catch (IOException) {
               // An instance of the egg is probably already running.
            }
         }

         string nestMainAppConfigPath = Path.Combine(localEgg.RootPath, "nest-main.dll.config");
         string nestHostAppConfigPath = Path.Combine(localEgg.RootPath, nestHostFileInfo.Name + ".config");
         if (File.Exists(nestMainAppConfigPath)) {
            File.Copy(nestMainAppConfigPath, nestHostAppConfigPath, true);
         }
      }
   }
}