using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface ReadableNestDirectory {
      NestContext GetNestContextByName(string name);
      IEnumerable<NestContext> EnumerateNests();
   }

   public interface ManageableNestDirectory : ReadableNestDirectory {
      void Register(NestContext nest);
      void Unregister(NestContext nest);
   }

   public class NestDirectory : ManageableNestDirectory {
      private readonly IConcurrentDictionary<string, NestContext> nestsByName = new ConcurrentDictionary<string, NestContext>();

      public void Register(NestContext nest) {
         nestsByName.Add(nest.Name, nest);
      }

      public void Unregister(NestContext nest) {
         nestsByName.TryRemove(nest.Name, nest);
      }

      public NestContext GetNestContextByName(string name) {
         NestContext nestContext;
         if (!nestsByName.TryGetValue(name, out nestContext)) {
            throw new KeyNotFoundException($"Could not find nest of name {name}!");
         }
         return nestContext;
      }

      public IEnumerable<NestContext> EnumerateNests() => nestsByName.Values;
   }
}
