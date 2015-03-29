using System.IO;
using Dargon.Nest.Exeggutor.Host.PortableObjects;
using Dargon.PortableObjects;
using ItzWarty.IO;
using ItzWarty.Processes;

namespace Dargon.Nest.Exeggutor {
   public interface RemoteHostContext {
      string Name { get; }
   }

   public class RemoteHostContextImpl : RemoteHostContext {
      private readonly IPofSerializer nestSerializer; 
      private readonly string name;
      private readonly string eggPath;
      private readonly IProcess process;
      private readonly IBinaryReader reader;
      private readonly IBinaryWriter writer;

      public RemoteHostContextImpl(IPofSerializer nestSerializer, string name, string eggPath, IProcess process, IBinaryReader reader, IBinaryWriter writer) {
         this.name = name;
         this.eggPath = eggPath;
         this.process = process;
         this.reader = reader;
         this.writer = writer;
         this.nestSerializer = nestSerializer;
      }

      public string Name { get { return name; } }

      public void Initialize() {

      }

      public void Bootstrap(byte[] arguments) {
         nestSerializer.Serialize(writer, new BootstrapDto(name, eggPath, arguments));
      }
   }
}