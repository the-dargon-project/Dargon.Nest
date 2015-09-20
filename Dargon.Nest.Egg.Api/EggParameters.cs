namespace Dargon.Nest.Egg {
   public class EggParameters : IEggParameters {
      public EggParameters(IEggHost host, string instanceName, byte[] arguments) {
         Host = host;
         InstanceName = instanceName;
         Arguments = arguments;
      }

      public IEggHost Host { get; private set; }
      public string InstanceName { get; private set; }
      public byte[] Arguments { get; private set; }
   }
}
