using Dargon.Nest.Daemon.Hatchlings;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Dargon.Nest.Daemon.Updating {
   public interface UpdateFetcher {
      Task FetchUpdatesAsync();
      Task FetchUpdatesForNestAsync(string nestName);
   }

   public class UpdateFetcherImpl : UpdateFetcher {
      private readonly WebClient webClient = new WebClient();
      private readonly NestDirectory nestDirectory;

      public UpdateFetcherImpl(NestDirectory nestDirectory) {
         this.nestDirectory = nestDirectory;
      }

      public Task FetchUpdatesAsync() {
         return Task.WhenAll(nestDirectory.EnumerateNests().Select(FetchUpdatesForNestAsync));
      }

      public Task FetchUpdatesForNestAsync(string nestName) {
         var nestContext = nestDirectory.GetNestContextByName(nestName);
         return FetchUpdatesForNestAsync(nestContext);
      }

      public async Task FetchUpdatesForNestAsync(NestContext nest) {
         await webClient.DownloadStringTaskAsync(nest.Remote);
      }
   }
}
