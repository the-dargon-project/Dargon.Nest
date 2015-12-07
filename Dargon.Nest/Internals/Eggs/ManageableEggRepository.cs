using System.Threading.Tasks;

namespace Dargon.Nest.Internals.Eggs {
   public interface ManageableEggRepository : ReadableEggRepository {
      Task SyncAsync(ReadableEggRepository remote);
   }
}