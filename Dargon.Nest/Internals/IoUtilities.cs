using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Dargon.Nest.Internals {
   public static class IoUtilities {
      public static string ReadString(string absolutePath) {
         return Encoding.UTF8.GetString(ReadBytes(absolutePath));
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
         return Path.GetFullPath(absolutePath).TrimEnd('/', '\\');
      }

      public static string GetDescendentRelativePath(string basePath, string descendentPath) {
         const int kLeadingSlashLength = 1;
         return FormatSystemPath(descendentPath).Substring(FormatSystemPath(basePath).Length + kLeadingSlashLength);
      }

      public static byte[] ReadBytes(string absolutePath) {
         if (File.Exists(absolutePath)) {
            return File.ReadAllBytes(absolutePath);
         } else {
            throw new NotSupportedException();
         }
      }

      public static string ExtractNameFromPath(string path) {
         path = path.Trim('/', '\\');
         var delimiterIndex = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
         return path.Substring(delimiterIndex + 1);
      }

      public static string CombinePath(string basePath, string extPath) {
         return Path.Combine(basePath, extPath);
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

      public static DirectoryInfo GetAncestorInfoOfName(string startPath, string ancestorName) {
         var currentDirectory = new FileInfo(startPath).Directory;
         while (currentDirectory != null && !currentDirectory.Name.Equals(ancestorName, StringComparison.OrdinalIgnoreCase)) {
            currentDirectory = currentDirectory.Parent;
         }
         return currentDirectory;
      }
   }
}