namespace Dargon.Nest.Egg {
   public interface IEggParameters {
      IEggHost Host { get; }
      string InstanceName { get; }
      byte[] Arguments { get; }
   }
}