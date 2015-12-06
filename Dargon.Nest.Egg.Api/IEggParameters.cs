namespace Dargon.Nest.Eggs {
   public interface IEggParameters {
      IEggHost Host { get; }
      string InstanceName { get; }
      byte[] Arguments { get; }
   }
}