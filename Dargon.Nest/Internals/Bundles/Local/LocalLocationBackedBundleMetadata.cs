namespace Dargon.Nest.Internals.Bundles.Local {
   public class LocalLocationBackedBundleMetadata : ReadableBundleMetadata {
      private readonly string location;

      public LocalLocationBackedBundleMetadata(string location) {
         this.location = location;
      }

      public string Name => IoUtilities.ExtractNameFromPath(location);
      public string Version => IoUtilities.ReadString(IoUtilities.CombinePath(location, NestConstants.kVersionFileName));
      public string Channel => IoUtilities.ReadString(IoUtilities.CombinePath(location, NestConstants.kChannelFileName));
      public string Remote => IoUtilities.ReadString(IoUtilities.CombinePath(location, NestConstants.kRemoteFileName));
   }
}
