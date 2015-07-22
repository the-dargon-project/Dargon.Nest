using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest;
using Microsoft.Win32;

namespace nest_spawner {
   class Program {
      public static void Main(string[] args) {
         var nestSpawnerExecutablePath = Assembly.GetEntryAssembly().Location;

         // Configure Run at Startup
         RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
         rk.SetValue("Dargon", nestSpawnerExecutablePath);

         // Update Nest Directory
         var nestDirectory = new FileInfo(nestSpawnerExecutablePath).Directory.Parent.FullName;
         var isInstallation = !File.Exists(Path.Combine(nestDirectory, "REMOTE"));

         var nest = new LocalDargonNest(nestDirectory);
         if (isInstallation || args.Any(s => s.Equals("--update"))) {
            nest.Remote = "http://dargon.io/files/nest";
            nest.Channel = "stable";
            nest.UpdateNest();
         }

         // Pull NestD
         nest.ExecuteEgg("nestd", "");

         // Start CoreD
         nest.ExecuteEgg("dev-egg-runner", "-e cored -n cored");
      }
   }
}
