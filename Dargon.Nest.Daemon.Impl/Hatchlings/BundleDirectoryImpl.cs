using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface ReadableBundleDirectory {
      BundleContext GetBundleContextByName(string name);
      IEnumerable<BundleContext> EnumerateBundles();
   }

   public interface ManageableBundleDirectory : ReadableBundleDirectory {
      void Register(BundleContext bundle);
      void Unregister(BundleContext bundle);
   }

   public class BundleDirectoryImpl : ManageableBundleDirectory {
      private readonly IConcurrentDictionary<string, BundleContext> bundlesByName = new ConcurrentDictionary<string, BundleContext>();

      public void Register(BundleContext bundle) {
         bundlesByName.Add(bundle.Name, bundle);
      }

      public void Unregister(BundleContext bundle) {
         bundlesByName.TryRemove(bundle.Name, bundle);
      }

      public BundleContext GetBundleContextByName(string name) {
         BundleContext bundleContext;
         if (!bundlesByName.TryGetValue(name, out bundleContext)) {
            throw new KeyNotFoundException($"Could not find nest of name {name}!");
         }
         return bundleContext;
      }

      public IEnumerable<BundleContext> EnumerateBundles() => bundlesByName.Values;
   }
}
