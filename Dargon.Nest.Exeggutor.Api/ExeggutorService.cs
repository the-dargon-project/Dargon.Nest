using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dargon.Nest.Eggs;

namespace Dargon.Nest.Eggxecutor {
   [Guid("6ED8503F-0908-4FEC-9E05-7E507B9274C1")]
   public interface ExeggutorService {
      SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration);
      Task<SpawnHatchlingResult> SpawnHatchlingAsync(string eggName, SpawnConfiguration configuration);

      IEnumerable<HatchlingStateDto> EnumerateHatchlings();
      Task<IEnumerable<HatchlingStateDto>> EnumerateHatchlingsAsync();

      void KillAllHatchlings(ShutdownReason reason);
      Task KillAllHatchlingsAsync(ShutdownReason reason);

      void KillHatchlingsOfBundles(ShutdownReason reason, params string[] bundleNames);
      Task KillHatchlingsOfBundlesAsync(ShutdownReason reason, params string[] bundleNames);

      void RunUpdateCycle();
      Task RunUpdateCycleAsync();
   }
}
