﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public static class NestUtil {
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

      public static string CombineUrl(string a, string b) {
         return new Uri(Path.Combine(a, b).Replace('\\', '/')).AbsoluteUri;
      }


      /// <summary>
      /// Quotation aware string split.  Will not break up 'words contained in quotes'... useful for handling console
      /// such as: del "C:\Derp a de herp\Lerp a merp\"
      /// 
      /// From libwarty
      /// </summary>
      public static string[] QASS(this string s, char delimiter = ' ') {
         StringBuilder curPartSB = new StringBuilder();
         List<string> finalParts = new List<string>();
         bool inDoubleQuotes = false;
         bool inSingleQuotes = false;
         for (int i = 0; i < s.Length; i++) {
            if (s[i] == '"')
               if (!inSingleQuotes)
                  inDoubleQuotes = !inDoubleQuotes;
               else
                  curPartSB.Append(s[i]);
            else if (s[i] == '\'')
               if (!inDoubleQuotes)
                  inSingleQuotes = !inSingleQuotes;
               else
                  curPartSB.Append(s[i]);
            else if (s[i] == delimiter) {
               if (!inDoubleQuotes && !inSingleQuotes) {
                  if (curPartSB.ToString() != "") {
                     finalParts.Add(curPartSB.ToString());
                     curPartSB.Clear();
                  }
               } else {
                  curPartSB.Append(s[i]);
               }
            } else
               curPartSB.Append(s[i]);
         }
         if (curPartSB.ToString() != "") {
            finalParts.Add(curPartSB.ToString());
         }
         return finalParts.ToArray();
      }
   }
}
