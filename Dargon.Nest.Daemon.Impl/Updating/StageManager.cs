using System.IO;
using Dargon.Nest.Daemon.Utilities;

namespace Dargon.Nest.Daemon.Updating {
   public interface StageManager {
      bool IsUpdateStaged(string nestName);
      void ProcessStagedUpdate(string nestName);
   }

   public class StageManagerImpl : StageManager {
      private const string kReadyFileName = "READY";

      private readonly FileUtilities fileUtilities;
      private readonly DirectoryInfo stageDirectoryInfo;
      private readonly DirectoryInfo nestsDirectoryInfo;

      public StageManagerImpl(FileUtilities fileUtilities, DirectoryInfo stageDirectoryInfo, DirectoryInfo nestsDirectoryInfo) {
         this.fileUtilities = fileUtilities;
         this.stageDirectoryInfo = stageDirectoryInfo;
         this.nestsDirectoryInfo = nestsDirectoryInfo;
      }

      public bool IsUpdateStaged(string nestName) {
         var readyFilePath = Path.Combine(stageDirectoryInfo.FullName, nestName, kReadyFileName);
         return File.Exists(readyFilePath);
      }

      public void ProcessStagedUpdate(string nestName) {
         var stagedNest = new DirectoryInfo(Path.Combine(stageDirectoryInfo.FullName, nestName));
         var targetNest = new DirectoryInfo(Path.Combine(nestsDirectoryInfo.FullName, nestName));
         var stagedReadyFilePath = Path.Combine(stagedNest.FullName, kReadyFileName);
         if (File.Exists(stagedReadyFilePath)) {
            fileUtilities.CopyReplaceDirectory(stagedNest, targetNest);
            File.Delete(stagedReadyFilePath);
         }
         stagedNest.Delete();
      }
   }
}