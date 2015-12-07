namespace Dargon.Nest.Internals.Bundles.Local {
   public class LocalLocationBackedBundleMetadata : ReadableBundleMetadata {
      private readonly string location;

      public LocalLocationBackedBundleMetadata(string location) {
         this.location = location;
      }

      public string Name => IoUtilities.ExtractNameFromPath(location);
      public string Version => IoUtilities.ReadStringOrFallbackAsync(IoUtilities.CombinePath(location, NestConstants.kVersionFileName)).Result;
      public string Remote => IoUtilities.ReadStringOrFallbackAsync(IoUtilities.CombinePath(location, NestConstants.kRemoteFileName)).Result;
      public string InitScript => IoUtilities.ReadStringOrFallbackAsync(IoUtilities.CombinePath(location, NestConstants.kInitJsonFileName)).Result;
   }
}
