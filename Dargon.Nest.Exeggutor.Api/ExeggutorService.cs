using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Dargon.Nest.Eggxecutor {
   [Guid("6ED8503F-0908-4FEC-9E05-7E507B9274C1")]
   public interface ExeggutorService {
      SpawnHatchlingResult SpawnHatchling(string eggName, SpawnConfiguration configuration);
      Task<SpawnHatchlingResult> SpawnHatchlingAsync(string eggName, SpawnConfiguration configuration);

      IEnumerable<HatchlingStateDto> EnumerateHatchlings();
      Task<IEnumerable<HatchlingStateDto>> EnumerateHatchlingsAsync();

      void KillHatchlings();
      Task KillHatchlingsAsync();

      void KillHatchlingsAndUpdateAllPackages();
      Task KillHatchlingsAndUpdateAllPackagesAsync();
   }
}
