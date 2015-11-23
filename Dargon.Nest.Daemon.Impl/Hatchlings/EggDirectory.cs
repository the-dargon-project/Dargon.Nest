using Fody.Constructors;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.Nest.Daemon.Hatchlings {
   [RequiredFieldsConstructor]
   public class EggDirectory {
      private readonly NestDirectoryImpl nestDirectory = null;

      public EggContext GetContextByName(string name) {
         if (name.Contains('/')) {
            return GetContextByFullName(name);
         } else {
            return GetContextByUnqualifiedName(name);
         }
      }

      private EggContext GetContextByFullName(string name) {
         var delimiterIndex = name.IndexOf('/');
         var nestName = name.Substring(0, delimiterIndex);
         var eggName = name.Substring(delimiterIndex + 1);
         var nest = nestDirectory.GetNestContextByName(nestName);
         return nest.GetEggByName(eggName);
      }

      private EggContext GetContextByUnqualifiedName(string name) {
         EggContext egg = null;
         if (!nestDirectory.EnumerateNests().Any(n => n.TryGetEggByName(name, out egg))) {
            throw new KeyNotFoundException($"Could not find egg of name {name}!");
         }
         return egg;
      }
   }
}