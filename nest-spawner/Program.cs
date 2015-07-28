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

namespace nest_spawner {
   class Program {
      private const string kNestSpawnerDirectoryName = "nest-spawner";
      private const string kNestSpawnerExecutableName = "nest-spawner.exe";

      public static void Main(string[] args) {
         var nestSpawnerExecutablePath = Assembly.GetEntryAssembly().Location;
         var nestSpawnerDirectory = new FileInfo(nestSpawnerExecutablePath).Directory;

         if (!nestSpawnerDirectory.Name.Equals(kNestSpawnerDirectoryName, StringComparison.OrdinalIgnoreCase)) {
            new Thread(Installer) { ApartmentState = ApartmentState.STA, IsBackground = false}.Start();
         } else {
            // Configure Run at Startup
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("Dargon", nestSpawnerExecutablePath);

            // Update Nest Directory
            var nestDirectoryPath = nestSpawnerDirectory.Parent.FullName;
            var isInstallation = !File.Exists(Path.Combine(nestDirectoryPath, "REMOTE"));

            var nest = new LocalDargonNest(nestDirectoryPath);
            if (isInstallation || args.Any(s => s.Equals("--update"))) {
               nest.Remote = "http://dargon.io/files/nest";
               nest.Channel = "stable";

               var updateNestOptions = new UpdateNestOptions();
               var updateState = new UpdateState();
               updateNestOptions.UpdateState = updateState;
               Dispatcher uiThreadDispatcher = null;
               var uiThreadReadySignal = new ManualResetEvent(false);
               new Thread(() => {
                  new UpdateStatusWindow() { DataContext = updateState }.Show();
                  uiThreadDispatcher = Dispatcher.CurrentDispatcher;
                  uiThreadReadySignal.Set();
                  Dispatcher.Run();
               }) { ApartmentState = ApartmentState.STA }.Start();
               uiThreadReadySignal.WaitOne();
               nest.UpdateNest(updateNestOptions);
               uiThreadDispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
               uiThreadDispatcher.Thread.Join();
            }

            // Pull NestD
            nest.ExecuteEgg("nestd", "");

            // Start CoreD
            nest.ExecuteEgg("dev-egg-runner", "-e cored -n cored");
         }
      }

      private static void Installer() {
         var dargonDirectoryDialog = new FolderBrowserDialog();
         dargonDirectoryDialog.Description = "Where do you want to install Dargon?";
         dargonDirectoryDialog.RootFolder = Environment.SpecialFolder.MyComputer;
         dargonDirectoryDialog.SelectedPath = "C:/Dargon";
         dargonDirectoryDialog.ShowNewFolderButton = true;

         Console.WriteLine("Showing");
         var dialogResult = dargonDirectoryDialog.ShowDialog();
         if (dialogResult == DialogResult.OK) {
            var nestPath = dargonDirectoryDialog.SelectedPath;
            Directory.CreateDirectory(nestPath);

            var nestDirectoryInfo = new DirectoryInfo(nestPath);
            if (nestDirectoryInfo.Exists &&
                !nestDirectoryInfo.Name.Equals("Dargon", StringComparison.OrdinalIgnoreCase) &&
                nestDirectoryInfo.EnumerateFileSystemInfos().Any() &&
                !File.Exists(Path.Combine(nestPath, "LOCK"))) {
               nestPath = Path.Combine(nestPath, "Dargon");
               Directory.CreateDirectory(nestPath);
            }

            var spawnerPath = Path.Combine(nestPath, kNestSpawnerDirectoryName);
            Directory.CreateDirectory(spawnerPath);

            var entryAssembly = Assembly.GetEntryAssembly();
            var clonedSpawnerPath = Path.Combine(spawnerPath, kNestSpawnerExecutableName);
            File.Copy(entryAssembly.Location, clonedSpawnerPath, true);

            var referencedAssemblies = entryAssembly.GetReferencedAssemblies();
            foreach (var referencedAssembly in referencedAssemblies) {
               var assemblyLocation = Assembly.Load(referencedAssembly.FullName).Location;
               var excludeRegex = new Regex("microsoft|windows|\\.net", RegexOptions.IgnoreCase);
               if (!excludeRegex.IsMatch(assemblyLocation)) {
                  File.Copy(assemblyLocation, Path.Combine(spawnerPath, new FileInfo(assemblyLocation).Name), true);
               }
            }

            Application.ApplicationExit += (s, e) => {
               Process.Start(new ProcessStartInfo {
                  FileName = clonedSpawnerPath,
                  UseShellExecute = true,
                  Arguments = "--update"
               });
            };

            Application.Exit();
         }
      }
   }
}
