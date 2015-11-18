using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest.Daemon {
   public class DaemonConfiguration {
      public string NestsPath { get; set; }
      public string StagePath { get; set; }
      public string HostExecutablePath { get; set; }

      public override string ToString() => $"[DaemonConfiguration Nests = \"{NestsPath}\", Stage = \"{StagePath}\", Host = \"{HostExecutablePath}\"]";
   }
}
