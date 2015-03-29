using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;

namespace Dargon.Nest {
   public interface IDargonEgg {
      string Name { get; }
      string Location { get; }
      string Version { get; }
      string Remote { get; }
      IReadOnlyList<EggFileListEntry> Files { get; }
      Stream GetStream(string internalPath);
      bool IsValid();
   }
}