using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public static class EggFactory {
      public static Egg CreateEggFromRemote(string remote) {
         if (File.Exists(remote)) {
            return new LocalEgg(remote);
         } else {
            return RemoteEgg.FromUrl(remote);
         }
      }
   }
}
