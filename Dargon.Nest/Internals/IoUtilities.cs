using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Internals {
   public static class IoUtilities {
      public static async Task<string> ReadStringAsync(string absolutePath) {
         return Encoding.UTF8.GetString(await ReadBytesAsync(absolutePath));
      }

      public static async Task<string> ReadStringOrFallbackAsync(string absolutePath, string fallback = "") {
         try {
            return Encoding.UTF8.GetString(await ReadBytesAsync(absolutePath));
         } catch (Exception e) {
            Console.Error.WriteLine(absolutePath);
            Console.Error.WriteLine(e);
            return fallback;
         }
      }

      public static void PrepareDirectory(string path) {
         Directory.CreateDirectory(path);
      }

      public static void PrepareParentDirectory(string path) {
         var parts = path.Split('/', '\\');
         var result = new StringBuilder(parts[0]);
         for (var i = 1; i < parts.Length - 1; i++) {
            result.Append("/");
            result.Append(parts[i]);
         }
         PrepareDirectory(result.ToString());
      }

      public static string FormatSystemPath(string absolutePath) {
         return Path.GetFullPath(absolutePath).Replace('\\', '/').TrimEnd('/');
      }

      public static string FormatWebPath(string absolutePath) {
         return new Uri(absolutePath.Replace('\\', '/').TrimEnd('/')).AbsoluteUri;
      }

      public static string GetDescendentRelativePath(string basePath, string descendentPath) {
         const int kLeadingSlashLength = 1;
         return FormatSystemPath(descendentPath).Substring(FormatSystemPath(basePath).Length + kLeadingSlashLength);
      }

      public static async Task<byte[]> ReadBytesAsync(string absolutePath) {
         if (IsLocal(absolutePath)) {
            using (var fs = File.OpenRead(absolutePath)) {
               var result = new byte[fs.Length];
               await fs.ReadAsync(result, 0, result.Length);
               return result;
            }
         } else {
            return await new WebClient().DownloadDataTaskAsync(FormatWebPath(absolutePath));
         }
      }

      public static string ExtractNameFromPath(string path) {
         path = path.Trim('/', '\\');
         var delimiterIndex = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
         return path.Substring(delimiterIndex + 1);
      }

      public static string CombinePath(string basePath, params string[] extPaths) {
         foreach (var extPath in extPaths) {
            basePath = Path.Combine(basePath, extPath);
         }
         return basePath;
      }

      public static bool IsLocal(string path) {
         return !path.StartsWith("http", StringComparison.OrdinalIgnoreCase);
      }

      public static Guid ComputeLocalFileHash(string localPath) {
         using (var md5 = MD5.Create())
         using (var stream = File.Open(localPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            var hash = md5.ComputeHash(stream);
            return new Guid(hash);
         }
      }

      public static string FindAncestorCachePath(string startPath) {
         var currentDirectory = new FileInfo(startPath).Directory;
         while (currentDirectory != null) {
            var cacheDirectoryPath = Path.Combine(currentDirectory.FullName, NestConstants.kCacheDirectoryName);
            if (Directory.Exists(cacheDirectoryPath)) {
               return cacheDirectoryPath;
            }
            currentDirectory = currentDirectory.Parent;
         }
         return null;
      }
   }
}