using System.Collections.Generic;
using System.IO;

namespace Dargon.Nest {
   public interface Egg {
      string Name { get; }
      string Location { get; }
      string Version { get; }
      string Remote { get; }
      IReadOnlyList<EggFileListEntry> Files { get; }
      Stream GetStream(string internalPath);
      bool IsValid();
   }
}