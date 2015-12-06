namespace Dargon.Nest.Internals.Eggs.InMemory {
   public class InMemoryEggMetadata : ReadableEggMetadata {
      public InMemoryEggMetadata(string name, string version) {
         Name = name;
         Version = version;
      }

      public string Name { get; }
      public string Version { get; }
      public string Remote => "";
   }
}
