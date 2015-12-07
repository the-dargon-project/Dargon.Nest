namespace Dargon.Nest.Eggs {
   public class HatchlingParameters {
      public HatchlingParameters(HatchlingHost host, string instanceName, byte[] arguments) {
         Host = host;
         InstanceName = instanceName;
         Arguments = arguments;
      }

      public HatchlingHost Host { get; private set; }
      public string InstanceName { get; private set; }
      public byte[] Arguments { get; private set; }
   }
}
