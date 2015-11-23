using System.IO;
using Dargon.Nest.Daemon.Utilities;
using Fody.Constructors;
using ItzWarty.IO;

namespace Dargon.Nest.Daemon.Updating {
   public interface StagedUpdateProcessor {
      bool IsUpdateStaged(string nestName);
      void ProcessDeadNestWithStagedUpdate(string nestName);
   }

   public class StagedUpdateProcessorImpl : StagedUpdateProcessor {
      private const string kReadyFileName = "READY";

      private readonly FileUtilities fileUtilities;
      private readonly IDirectoryInfo stageDirectoryInfo;
      private readonly IDirectoryInfo nestsDirectoryInfo;

      public StagedUpdateProcessorImpl(FileUtilities fileUtilities, IDirectoryInfo stageDirectoryInfo, IDirectoryInfo nestsDirectoryInfo) {
         this.fileUtilities = fileUtilities;
         this.stageDirectoryInfo = stageDirectoryInfo;
         this.nestsDirectoryInfo = nestsDirectoryInfo;
      }

      public bool IsUpdateStaged(string nestName) {
         var readyFilePath = Path.Combine(stageDirectoryInfo.FullName, nestName, kReadyFileName);
         return File.Exists(readyFilePath);
      }

      public void ProcessDeadNestWithStagedUpdate(string nestName) {
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