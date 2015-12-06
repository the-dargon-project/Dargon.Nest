using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Dargon.Nest {
   public class RemoteEgg : Egg {
      public static RemoteEgg FromUrl(string webUrl, string remoteUrl = null) {
         Console.WriteLine("Fetching Egg Info from " + webUrl);
         var webClient = new WebClient();
         string eggName;
         string eggVersion;

         webUrl = webUrl.Trim('/', '\\');
         var versionPattern = new Regex("\\d+(?:\\.\\d+)+"); // e.g. ...0.0.3
         var nameWithVersionPattern = new Regex("^[a-zA-Z-_]+-\\d+(?:\\.\\d+)+$"); // e.g. daemon-impl-0.0.3
         var breadCrumb = webUrl.Substring(webUrl.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
         if (!nameWithVersionPattern.IsMatch(breadCrumb)) {
            // read the file; it'll point to the real egg.
            var nextHop = webClient.DownloadString(webUrl);
            Console.Write("Hop: " + nextHop);
            Console.WriteLine(" => " + (nextHop = NestUtil.CombineUrl(webUrl + "/", nextHop)));
            return FromUrl(nextHop, remoteUrl ?? webUrl);
         } else {
            var match = versionPattern.Match(breadCrumb);
            eggVersion = match.Value;
            eggName = breadCrumb.Substring(0, breadCrumb.Length - eggVersion.Length - 1); // -1 to get rid of last -
         }

         // download file list
         var fileList = EggFileListSerializer.Deserialize(webClient.DownloadString(NestUtil.CombineUrl(webUrl, "filelist")));
         
         return new RemoteEgg(webUrl, eggName, remoteUrl ?? webUrl, eggVersion, fileList);
      }

      private readonly WebClient webClient = new WebClient();
      private readonly string url;
      private readonly string name;
      private readonly string remote;
      private readonly string version;
      private readonly IReadOnlyList<EggFileListEntry> files;

      private RemoteEgg(string url, string name, string remote, string version, IReadOnlyList<EggFileListEntry> files) {
         this.url = url;
         this.name = name;
         this.remote = remote;
         this.version = version;
         this.files = files;
      }

      public string Name { get { return name; } }
      public string Location { get { return remote; } }
      public string Version { get { return version; } }
      public string Remote { get { return remote; } }

      public IReadOnlyList<EggFileListEntry> Files { get { return files; } }

      public Stream GetStream(string internalPath) {
         return webClient.OpenRead(url + "/" + internalPath);
      }

      public bool IsValid() {
         return true;
      }
   }
}