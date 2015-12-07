using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Internals;
using Dargon.Nest.Internals.Deployment;
using Dargon.Nest.Internals.Deployment.Local;

namespace Dargon.Nest {
   public static class DeploymentFactory {
      public static ManageableDeployment Local(string path) {
         path = IoUtilities.FormatSystemPath(path);
         return new ManageableDeploymentProxy(
            new LocalDeploymentMetadata(path),
            new LocalBundleContainer(path));
      }

      public static async Task<ReadableDeployment> RemoteLatestAsync(string remote, string deploymentName, string channelName) {
         var deploymentPath = IoUtilities.CombinePath(remote, NestConstants.kDeploymentsDirectoryName, deploymentName);
         var latestReleaseVersion = await IoUtilities.ReadStringAsync(IoUtilities.CombinePath(deploymentPath, NestConstants.kChannelsDirectoryName, channelName));
         return RemoteVersion(remote, deploymentName, channelName, latestReleaseVersion);
      }

      public static ReadableDeployment RemoteVersion(string remote, string deploymentName, string channelName, string version) {
         var deploymentPath = IoUtilities.CombinePath(remote, NestConstants.kDeploymentsDirectoryName, deploymentName);
         var latestReleasePath = IoUtilities.CombinePath(deploymentPath, NestConstants.kReleasesDirectoryName, $"{deploymentName}-{version}");
         return new ReadableDeploymentProxy(
            new RemoteDeploymentMetadata(deploymentName, version, remote, channelName),
            new RemoteBundleContainer(deploymentName, version, remote, channelName));
      }
   }

   public class RemoteBundleContainer : ReadableBundleContainer {
      private readonly string name;
      private readonly string version;
      private readonly string remote;
      private readonly string channel;

      public RemoteBundleContainer(string name, string version, string remote, string channel) {
         this.name = name;
         this.version = version;
         this.remote = remote;
         this.channel = channel;
         Location = IoUtilities.CombinePath(remote, NestConstants.kDeploymentsDirectoryName, name, NestConstants.kReleasesDirectoryName, $"{name}-{version}");
      }

      public string Location { get; }

      public async Task<IEnumerable<ReadableBundle>> EnumerateBundlesAsync() {
         var bundlesString = await IoUtilities.ReadStringAsync(IoUtilities.CombinePath(Location, NestConstants.kBundlesFileName));
         return from line in bundlesString.Split('\n')
                let trimmedLine = line.Trim()
                let parts = trimmedLine.Split(' ')
                let bundleName = parts[0]
                let bundleVersion = parts[1]
                select BundleFactory.Remote(bundleName, bundleVersion, remote);
      }
   }

   public class RemoteDeploymentMetadata : ReadableDeploymentMetadata {
      public RemoteDeploymentMetadata(string name, string version, string remote, string channel) {
         Name = name;
         Version = version;
         Remote = remote;
         Channel = channel;
      }

      public string Name { get; }
      public string Version { get; }
      public string Remote { get; }
      public string Channel { get; }
   }
}
