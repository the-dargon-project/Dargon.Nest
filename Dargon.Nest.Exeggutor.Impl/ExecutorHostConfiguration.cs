namespace Dargon.Nest.Exeggutor {
   public interface ExecutorHostConfiguration {
      string HostExecutablePath { get; }
   }

   public class ExecutorHostConfigurationImpl : ExecutorHostConfiguration {
      public ExecutorHostConfigurationImpl(string hostExecutablePath) {
         this.HostExecutablePath = hostExecutablePath;
      }

      public string HostExecutablePath { get; set; }
   }
}