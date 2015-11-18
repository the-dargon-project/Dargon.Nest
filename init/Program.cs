using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;

namespace Nest.Init {
   public static class Program {
      private static readonly char[] kPathDelimiters = { '/', '\\' };
      private const string kReadyFileName = "READY";

      private const string kStageDirectoryName = "stage";
      private const string kNestsDirectoryName = "nests";
      private const string kTempDirectoryName = "temp";
      private const string kLogsDirectoryName = "logs";
      private const string kNestDaemonRelativePath = kNestsDirectoryName + "/nest/nestd/nestd.exe";

      public static void Main(string[] args) {
         var initAssembly = Assembly.GetExecutingAssembly();
         var rootDirectory = new FileInfo(initAssembly.Location).Directory;
         Debug.Assert(rootDirectory != null);

         var nestsDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, kNestsDirectoryName));
         if (!nestsDirectory.Exists) {
            nestsDirectory.Create();
         }

         var stageDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, kStageDirectoryName));
         if (stageDirectory.Exists) {
            ProcessStagedNests(stageDirectory, nestsDirectory);
            ClearDirectory(stageDirectory);
         } else {
            stageDirectory.Create();
         }

         var tempDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, kTempDirectoryName));
         if (tempDirectory.Exists) {
            ClearDirectory(tempDirectory);
         } else {
            tempDirectory.Create();
         }

         var logsDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, kLogsDirectoryName));
         if (logsDirectory.Exists) {
            CompactLogs(logsDirectory);
         } else {
            logsDirectory.Create();
         }
         
         var nestDaemonPath = Path.Combine(rootDirectory.FullName, kNestDaemonRelativePath);
         if (File.Exists(nestDaemonPath)) {
            Process.Start(
               new ProcessStartInfo(nestDaemonPath, string.Join(" ", args)) {
                  WorkingDirectory = new FileInfo(nestDaemonPath).Directory.FullName
               });
         }
      }

      private static void CompactLogs(DirectoryInfo logsDirectory) {
         foreach (var subdirectory in logsDirectory.EnumerateDirectories()) {
            var logFiles = subdirectory.GetFiles("*", SearchOption.AllDirectories);
            if (logFiles.Length == 0) {
               continue;
            }

            var logsArchivePath = Path.Combine(logsDirectory.FullName, $"{subdirectory.Name}-logs.zip");
            using (var archive = OpenOrCreateZipArchiveForUpdating(logsArchivePath)) {
               foreach (var logFile in logFiles) {
                  MergeIntoArchive(archive, subdirectory, logFile);
               }
            }
            ClearDirectory(subdirectory);
         }
      }

      private static void MergeIntoArchive(ZipArchive archive, DirectoryInfo nestLogDirectory, FileInfo logFile) {
         var internalPath = logFile.FullName.Substring(nestLogDirectory.FullName.Length).Trim(kPathDelimiters);
         var entry = archive.GetEntry(internalPath) ?? archive.CreateEntry(internalPath, CompressionLevel.Fastest);

         using (var dataStream = entry.Open())
         using (var writer = new StreamWriter(dataStream, Encoding.UTF8)){
            dataStream.Seek(0, SeekOrigin.End);
            writer.Write(File.ReadAllText(logFile.FullName));
         }
      }

      private static ZipArchive OpenOrCreateZipArchiveForUpdating(string logsArchivePath) {
         var fs = new FileStream(logsArchivePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
         try {
            return new ZipArchive(fs, ZipArchiveMode.Create, false);
         } catch (InvalidDataException) {
            fs.SetLength(0);
            return new ZipArchive(fs, ZipArchiveMode.Create, false);
         }
      }

      private static void ProcessStagedNests(DirectoryInfo stageDirectory, DirectoryInfo nestsDirectory) {
         foreach (var stagedNest in stageDirectory.EnumerateDirectories()) {
            var existingNest = new DirectoryInfo(Path.Combine(nestsDirectory.FullName, stagedNest.Name));
            MigrateAndDeleteStagedNest(stagedNest, existingNest);
         }
      }

      private static void MigrateAndDeleteStagedNest(DirectoryInfo stagedNest, DirectoryInfo targetNest) {
         var stagedReadyFilePath = Path.Combine(stagedNest.FullName, kReadyFileName);
         if (File.Exists(stagedReadyFilePath)) {
            CopyReplaceDirectory(stagedNest, targetNest);
            File.Delete(stagedReadyFilePath);
         }
         stagedNest.Delete();
      }

      private static void CopyReplaceDirectory(DirectoryInfo source, DirectoryInfo destination) {
         ClearDirectory(destination);

         foreach (var sourceFile in source.EnumerateFiles()) {
            var destinationFilePath = Path.Combine(destination.FullName, sourceFile.Name);
            sourceFile.CopyTo(destinationFilePath);
         }

         foreach (var sourceSubdirectory in source.EnumerateDirectories()) {
            var destinationSubdirectoryPath = Path.Combine(destination.FullName, sourceSubdirectory.Name);
            var destinationSubdirectory = new DirectoryInfo(destinationSubdirectoryPath);
            CopyReplaceDirectory(sourceSubdirectory, destinationSubdirectory);
         }
      }

      private static void ClearDirectory(DirectoryInfo destination) {
         foreach (var file in destination.EnumerateFiles()) {
            file.Delete();
         }
         foreach (var subdirectory in destination.EnumerateDirectories()) {
            subdirectory.Delete(true);
         }
      }
   }
}
