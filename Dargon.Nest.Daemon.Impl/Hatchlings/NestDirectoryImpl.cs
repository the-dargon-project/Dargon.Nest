using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface ReadableNestDirectory {
      BundleContext GetNestContextByName(string name);
      IEnumerable<BundleContext> EnumerateNests();
   }

   public interface ManageableNestDirectory : ReadableNestDirectory {
      void Register(BundleContext bundle);
      void Unregister(BundleContext bundle);
   }

   public class NestDirectoryImpl : ManageableNestDirectory {
      private readonly IConcurrentDictionary<string, BundleContext> nestsByName = new ConcurrentDictionary<string, BundleContext>();

      public void Register(BundleContext bundle) {
         nestsByName.Add(bundle.Name, bundle);
      }

      public void Unregister(BundleContext bundle) {
         nestsByName.TryRemove(bundle.Name, bundle);
      }

      public BundleContext GetNestContextByName(string name) {
         BundleContext bundleContext;
         if (!nestsByName.TryGetValue(name, out bundleContext)) {
            throw new KeyNotFoundException($"Could not find nest of name {name}!");
         }
         return bundleContext;
      }

      public IEnumerable<BundleContext> EnumerateNests() => nestsByName.Values;
   }
}
