using Dargon.Nest.Daemon.Hatchlings;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fody.Constructors;

namespace Dargon.Nest.Daemon.Updating {
   public interface UpdateFetcher {
      Task FetchUpdatesAsync();
      Task FetchUpdatesForNestAsync(string nestName);
   }

   [RequiredFieldsConstructor]
   public class UpdateFetcherImpl : UpdateFetcher {
      private readonly WebClient webClient = new WebClient();
      private readonly NestDirectoryImpl nestDirectory = null;

      public Task FetchUpdatesAsync() {
         return Task.WhenAll(nestDirectory.EnumerateNests().Select(FetchUpdatesForNestAsync));
      }

      public Task FetchUpdatesForNestAsync(string nestName) {
         var nestContext = nestDirectory.GetNestContextByName(nestName);
         return FetchUpdatesForNestAsync(nestContext);
      }

      public async Task FetchUpdatesForNestAsync(BundleContext bundle) {
         var remote = bundle.Remote;
         if (string.IsNullOrWhiteSpace(remote)) return;
         await webClient.DownloadStringTaskAsync(remote);
      }
   }
}
