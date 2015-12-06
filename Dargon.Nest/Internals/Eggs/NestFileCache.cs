using System;
using System.IO;
using System.Threading;

namespace Dargon.Nest.Internals.Eggs {
   public class NestFileCache {
      private readonly string path;

      public NestFileCache(string path) {
         this.path = path;
         IoUtilities.PrepareDirectory(path);
      }

      private static bool TryOpen(string path, out FileStream fileStream) {
         var spinner = new SpinWait();
         while (File.Exists(path)) {
            try {
               fileStream = File.OpenRead(path);
               return true;
            } catch (FileNotFoundException) {
               // file got deleted on us
            } catch (IOException) {
               // file is being written to
               spinner.SpinOnce();
            }
         }
         fileStream = null;
         return false;
      }

      public FileStream OpenOrAddAndOpen(Guid guid, Func<Guid, byte[]> readContents) {
         var cacheFilePath = BuildPath(guid);

         while (true) {
            IoUtilities.PrepareParentDirectory(cacheFilePath);

            // try simply opening the cache file in read mode
            FileStream fileStream;
            if (TryOpen(cacheFilePath, out fileStream)) {
               return fileStream;
            }

            // cache file didn't exist, try pulling.
            try {
               using (var fs = File.Open(cacheFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                  var data = readContents(guid);
                  fs.Write(data, 0, data.Length);
               }
            } catch (IOException) {
               // Someone else opened the cache file for writing
            }
         }
      }

      private string BuildPath(Guid guid) => Path.Combine(path, guid.ToString("n"));
   }
}