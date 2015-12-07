using Dargon.Nest;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Dargon.Nest.Internals;

namespace nest_spawner {
   public class Program {
      private const string kRemoteUrl = "http://localhost"; // "https://packages.dargon.io";
      private const string kDeploymentName = "dargon-client";
      private const string kChannelName = "stable";
      private const string kDefaultInstallationFolderName = "Dargon";
      private const string kDefaultInstallationPath = "C:/" + kDefaultInstallationFolderName;
      private const string kInitEggName = "init";
      private const string kInitExeName = "init.exe";
      private const int kInitPort = 21999;

      [STAThread]
      public static void Main(string[] args) {
         var installationPath = GetInstallationPath();
         var deploymentLatest = DeploymentFactory.RemoteLatestAsync(kRemoteUrl, kDeploymentName, kChannelName).Result;
         var localNest = NestFactory.Local(installationPath);
         localNest.InstallDeploymentAsync(deploymentLatest).Wait();

         // install init, which isn't actually standard to nest
         var localEgg = EggFactory.Local(Path.Combine(localNest.Location, NestConstants.kDeploymentsDirectoryName, deploymentLatest.Name, kInitEggName));
         localEgg.SyncAsync(EggFactory.RemoteByChannelLatestAsync(kRemoteUrl, kInitEggName, kChannelName).Result).Wait();

         // run init.exe
         var initExePath = localEgg.ComputeFullPath(kInitExeName);
         Console.WriteLine(initExePath);
         var process = Process.Start(initExePath, "-p " + kInitPort);
         Console.WriteLine("Done " + process.Id);
      }

      private static string GetInstallationPath() {
         var installationPathDialog = new FolderBrowserDialog {
            Description = $"Select `{kDeploymentName}` Installation Directory",
            RootFolder = Environment.SpecialFolder.MyComputer,
            SelectedPath = kDefaultInstallationPath,
            ShowNewFolderButton = true
         };
         
         if (installationPathDialog.ShowDialog() != DialogResult.OK) {
            Environment.Exit(1);
         }

         var installationPath = installationPathDialog.SelectedPath;
         Directory.CreateDirectory(installationPath);

         // Move to ./Dargon if current directory occupied by other stuff
         var installationDirectoryInfo = new DirectoryInfo(installationPath);
         if (installationDirectoryInfo.Exists &&
             !installationDirectoryInfo.Name.Equals(kDefaultInstallationFolderName, StringComparison.OrdinalIgnoreCase) &&
             installationDirectoryInfo.EnumerateFileSystemInfos().Any() &&
             !File.Exists(Path.Combine(installationPath, "LOCK"))) {
            installationPath = Path.Combine(installationPath, kDefaultInstallationFolderName);
            Directory.CreateDirectory(installationPath);
         }

         return installationPathDialog.SelectedPath;
      }
   }
}
