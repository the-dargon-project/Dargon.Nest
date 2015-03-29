namespace Dargon.Nest.Exeggutor {
   public interface ExecutorHostConfiguration {
      string HostExecutablePath { get; }
   }

   public class ExecutorHostConfigurationImpl : ExecutorHostConfiguration {
      private readonly string hostExecutablePath;

      public ExecutorHostConfigurationImpl(string hostExecutablePath) {
         this.hostExecutablePath = hostExecutablePath;
      }

      public string HostExecutablePath { get { return hostExecutablePath; } }
   }
}