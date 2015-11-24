using Dargon.Nest.Daemon;
using Dargon.Nest.Eggxecutor;
using Dargon.Ryu;
using Dargon.Services;
using Dargon.Services.Clustering;
using ItzWarty;
using ItzWarty.IO;
using NMockito;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Xunit;
using static Dargon.Services.AsyncStatics;

namespace Dargon.Nest.Examples.Tests {
   public class ExamplesFT : NMockitoInstance {
      private const int kTestServicesPort = 40000;

      [Fact]
      public void Run() {
         var ryu = new RyuFactory().Create();
         ryu.Touch<ItzWartyProxiesRyuPackage>();
         ryu.Touch<ServicesRyuPackage>();
         ryu.Touch<ExeggutorApiRyuPackage>();
         ryu.Set<ClusteringConfiguration>(new ClusteringConfigurationImpl(
            IPAddress.Loopback,
            kTestServicesPort,
            ClusteringRole.HostOrGuest));
         ((RyuContainerImpl)ryu).Setup(true);

         var fileSystemProxy = ryu.Get<IFileSystemProxy>();

         var dargonPath = Path.Combine(Environment.CurrentDirectory, "Dargon");
         fileSystemProxy.PrepareDirectory(dargonPath);

         var nestsPath = Path.Combine(dargonPath, "nests");
         fileSystemProxy.PrepareDirectory(nestsPath);

         var exampleNestPath = Path.Combine(nestsPath, "example");
         fileSystemProxy.PrepareDirectory(exampleNestPath);

         var exampleNest = new LocalDargonNest(exampleNestPath);
         ImportProjectToNest(exampleNest, ".", "example-client");
         ImportProjectToNest(exampleNest, ".", "example-daemon");

         var nestNestPath = Path.Combine(nestsPath, "nest");
         fileSystemProxy.PrepareDirectory(nestNestPath);

         var nestNest = new LocalDargonNest(nestNestPath);
         ImportProjectToNest(nestNest, "..", "nestd");
         ImportProjectToNest(nestNest, "..", "nest-host");
         ImportProjectToNest(nestNest, "..", "dev-nest-commander");

         var initFileInfo = fileSystemProxy.GetFileInfo(BuildPath("..", "init", "init.exe"));
         var deployedInitFilePath = Path.Combine(dargonPath, initFileInfo.Name);
         File.Copy(initFileInfo.FullName, deployedInitFilePath, true);

         var initConfigFileInfo = fileSystemProxy.GetFileInfo(BuildPath(".", "Dargon.Nest.Examples.Tests", "example.init.json"));
         var deployedInitConfigFilePath = Path.Combine(exampleNestPath, "init.json");
         File.Copy(initConfigFileInfo.FullName, deployedInitConfigFilePath, true);

         Process.Start(
            new ProcessStartInfo(
               deployedInitFilePath, 
               $"-p {kTestServicesPort}") {
               UseShellExecute = true
            });

         Thread.Sleep(8000);

         var serviceClient = ryu.Get<ServiceClient>();
         var exeggutorService = serviceClient.GetService<ExeggutorService>();
         var hatchlings = exeggutorService.EnumerateHatchlings().ToArray();
         Console.WriteLine("Hatchling count: " + hatchlings.Length);
         AssertEquals(1, hatchlings.Length);

         var commanderFileInfo = fileSystemProxy.GetFileInfo(BuildPath("..", "dev-nest-commander", "dev-nest-commander.exe"));
         var exampleClientSpawner = Process.Start(
            new ProcessStartInfo(
               commanderFileInfo.FullName, 
               $"-c spawn-egg -e example/example-client -p {kTestServicesPort} -n example-client") {
               UseShellExecute = true
            });
         exampleClientSpawner.WaitForExit();
         hatchlings = exeggutorService.EnumerateHatchlings().ToArray();
         AssertEquals(2, hatchlings.Length);
         Thread.Sleep(5000);
         hatchlings = exeggutorService.EnumerateHatchlings().ToArray();
         AssertEquals(0, hatchlings.Length);
         exeggutorService.KillHatchlings();

         var nestDaemonService = serviceClient.GetService<NestDaemonService>();
         var shutdownTask = Async(() => nestDaemonService.WaitForShutdown());
         AssertFalse(shutdownTask.IsCompleted);
         nestDaemonService.KillDaemon();
         shutdownTask.Wait();
      }

      private void ImportProjectToNest(LocalDargonNest nest, string folderPath, string projectName) {
         var debugFolderPath = BuildPath(folderPath, projectName);
         var importableEgg = new InMemoryEgg(projectName, "dev", debugFolderPath);
         nest.InstallEgg(importableEgg);
      }

      private static string BuildPath(string folderPath, string projectName, string relative = "") {
         var testDllPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
         var fullFolderPath = Path.GetFullPath(Path.Combine(testDllPath, folderPath, "..", "..", "..", "..", projectName));
         var debugFolderPath = Path.Combine(fullFolderPath, "bin", "Debug", relative);
         return debugFolderPath;
      }
   }
}
