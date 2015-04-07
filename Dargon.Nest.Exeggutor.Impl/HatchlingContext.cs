using System;
using System.IO;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using ItzWarty.IO;
using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public interface HatchlingContext {
      Guid InstanceId { get; }
      string Name { get; }
      void Bootstrap(byte[] arguments);
   }

   public class HatchlingContextImpl : HatchlingContext {
      private readonly IPofSerializer nestSerializer;
      private readonly Guid instanceId;
      private readonly string name;
      private readonly string eggPath;
      private readonly IProcess process;
      private readonly IBinaryReader reader;
      private readonly IBinaryWriter writer;

      public HatchlingContextImpl(IPofSerializer nestSerializer, Guid instanceId, string name, string eggPath, IProcess process, IBinaryReader reader, IBinaryWriter writer) {
         this.name = name;
         this.eggPath = eggPath;
         this.process = process;
         this.reader = reader;
         this.writer = writer;
         this.nestSerializer = nestSerializer;
         this.instanceId = instanceId;
      }

      public Guid InstanceId { get { return instanceId; } }
      public string Name { get { return name; } }

      public void Initialize() {

      }

      public void Bootstrap(byte[] arguments) {
         nestSerializer.Serialize(writer, new BootstrapDto(name, eggPath, arguments));
         writer.Flush();
         Console.WriteLine("Wrote bootstrap dto");
      }
   }
}