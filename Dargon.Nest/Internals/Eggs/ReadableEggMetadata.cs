namespace Dargon.Nest.Internals.Eggs {
   public interface ReadableEggMetadata {
      string Name { get; }
      string Version { get; }
      string Remote { get; }
   }
}