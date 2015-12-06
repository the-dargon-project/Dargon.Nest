namespace Dargon.Nest.Internals.Eggs {
   public interface ManageableEggRepository : ReadableEggRepository {
      void Sync(ReadableEggRepository remote);
   }
}