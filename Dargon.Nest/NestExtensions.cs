using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dargon.Nest.Internals.Bundles;
using Dargon.Nest.Internals.Eggs;

namespace Dargon.Nest {
   public static class NestExtensions {
      public static void Sync(this ManageableEgg egg) {
         egg.Sync(EggFactory.FileBacked(egg.Remote));
      }

      public static bool ContainsEgg(this ReadableEggContainer container, string eggName) {
         ReadableEgg eggThrowaway;
         return TryGetEgg(container, eggName, out eggThrowaway);
      }

      public static ReadableEgg GetEgg(this ReadableEggContainer container, string eggName) {
         ReadableEgg result;
         if (!container.TryGetEgg(eggName, out result)) {
            throw new KeyNotFoundException($"Could not find egg of name `{eggName}`.");
         }
         return result;
      }

      public static ManageableEgg GetEgg(this ManageableEggContainer container, string eggName) {
         ManageableEgg result;
         if (!container.TryGetEgg(eggName, out result)) {
            throw new KeyNotFoundException($"Could not find egg of name `{eggName}`.");
         }
         return result;
      }

      public static ReadableEgg GetEggOrNull(this ReadableEggContainer container, string eggName) {
         ReadableEgg result;
         container.TryGetEgg(eggName, out result);
         return result;
      }

      public static ManageableEgg GetEggOrNull(this ManageableEggContainer container, string eggName) {
         ManageableEgg result;
         container.TryGetEgg(eggName, out result);
         return result;
      }

      public static bool TryGetEgg(this ReadableEggContainer container, string eggName, out ReadableEgg egg) {
         egg = container.EnumerateEggs().FirstOrDefault(x => x.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
         return egg != null;
      }

      public static bool TryGetEgg(this ManageableEggContainer container, string eggName, out ManageableEgg egg) {
         egg = container.EnumerateEggs().FirstOrDefault(x => x.Name.Equals(eggName, StringComparison.OrdinalIgnoreCase));
         return egg != null;
      }

      public static void InstallEgg(this ManageableEggContainer container, ReadableEgg remoteEgg) {
         ManageableEgg existingEgg;
         if (container.TryGetEgg(remoteEgg.Name, out existingEgg)) {
            existingEgg.Sync(remoteEgg);
         } else {
            EggOperations.Install(Path.Combine(container.Location, remoteEgg.Name), remoteEgg);
         }
      }

      public static void SyncEgg(this ManageableEggContainer container, string eggName) {
         container.GetEgg(eggName).Sync();
      }

      public static void SyncEggs(this ManageableEggContainer container) {
         foreach (var egg in container.EnumerateEggs()) {
            egg.Sync();
         }
      }
   }
}