namespace Dargon.Nest.Internals.Deployment {
   public interface ReadableDeploymentMetadata {
      string Name { get; }
      string Version { get; }
      string Remote { get; }
      string Channel { get; }
   }
}