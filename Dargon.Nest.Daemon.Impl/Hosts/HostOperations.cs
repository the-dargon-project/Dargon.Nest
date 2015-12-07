using Fody.Constructors;
using ItzWarty;
using ItzWarty.IO;
using System;
using System.IO;
using System.Linq;

namespace Dargon.Nest.Daemon.Hosts {
   [RequiredFieldsConstructor]
   public class HostOperations {
      private readonly IFileSystemProxy fileSystemProxy = null;
      private readonly DaemonConfiguration daemonConfiguration = null;
      
      public void CopyHostToLocalEgg(ManageableEgg localEgg) {
         var nestHostFileInfo = fileSystemProxy.GetFileInfo(daemonConfiguration.HostExecutablePath);
         var nestHostDirectory = nestHostFileInfo.Parent;
         var nestHostAssemblies = from file in nestHostDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                                  where file.Name.EndsWithAny(new[] { ".exe", ".dll" }, StringComparison.OrdinalIgnoreCase)
                                  select file;

         foreach (var assemblyFileInfo in nestHostAssemblies) {
            try {
               var relativePath = assemblyFileInfo.FullName.Substring(nestHostDirectory.FullName.Length + 1);
               var destinationPath = Path.Combine(localEgg.Location, relativePath);

               if (localEgg.EnumerateFilesAsync().Result.None(x => x.InternalPath.Equals(relativePath, StringComparison.OrdinalIgnoreCase))) {
                  File.Copy(assemblyFileInfo.FullName, destinationPath, true);
               }
            } catch (IOException) {
               // An instance of the egg is probably already running.
            }
         }

         string nestMainAppConfigPath = Path.Combine(localEgg.Location, "nest-main.dll.config");
         string nestHostAppConfigPath = Path.Combine(localEgg.Location, nestHostFileInfo.Name + ".config");
         if (File.Exists(nestMainAppConfigPath)) {
            File.Copy(nestMainAppConfigPath, nestHostAppConfigPath, true);
         }
      }
   }
}