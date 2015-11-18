using System.IO;

namespace Dargon.Nest.Examples.Tests {
   // TODO: This was copied from Dargon.Nest.Daemon.Impl
   public interface FileUtilities {
      void ClearDirectory(DirectoryInfo destination);
      void CopyReplaceDirectory(DirectoryInfo source, DirectoryInfo destination);
   }

   public class FileUtilitiesImpl : FileUtilities {
      public void ClearDirectory(DirectoryInfo destination) {
         foreach (var file in destination.EnumerateFiles()) {
            file.Delete();
         }
         foreach (var subdirectory in destination.EnumerateDirectories()) {
            subdirectory.Delete(true);
         }
      }

      public void CopyReplaceDirectory(DirectoryInfo source, DirectoryInfo destination) {
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
   }
}