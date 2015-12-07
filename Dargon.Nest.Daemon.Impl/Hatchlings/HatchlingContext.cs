using Dargon.Nest.Daemon.Hosts;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using System;
using System.Threading.Tasks;

namespace Dargon.Nest.Daemon.Hatchlings {
   public interface HatchlingContext {
      Guid Id { get; }
      string Name { get; }

      HostProcess Process { get; }
      BundleContext Bundle { get; }
      Task<NestResult> GetStartResultAsync();

      Task ShutdownAsync(ShutdownReason reason);

      Task<HatchlingStateDto> ToDataTransferObjectAsync();
   }

   public class HatchlingContextImpl : HatchlingContext {
      private readonly Guid id;
      private readonly HostProcess hostProcess;
      private readonly SpawnConfiguration spawnConfiguration;
      private readonly EggContext eggContext;
      private readonly ManageableHatchlingDirectory hatchlingDirectory;

      public HatchlingContextImpl(Guid id, HostProcess hostProcess, SpawnConfiguration spawnConfiguration, EggContext eggContext, ManageableHatchlingDirectory hatchlingDirectory) {
         this.id = id;
         this.hostProcess = hostProcess;
         this.spawnConfiguration = spawnConfiguration;
         this.eggContext = eggContext;
         this.hatchlingDirectory = hatchlingDirectory;
      }

      public void Initialize() {
         hatchlingDirectory.RegisterHatchling(this);
         hostProcess.Exited += (s, e) => hatchlingDirectory.UnregisterHatchling(this);
      }

      public Guid Id => id;
      public string Name => spawnConfiguration.InstanceName;

      public HostProcess Process => hostProcess;
      public BundleContext Bundle => eggContext.BundleContext;

      public Task<NestResult> GetStartResultAsync() => hostProcess.GetStartResultAsync();

      public Task ShutdownAsync(ShutdownReason reason) => hostProcess.ShutdownAsync(reason);

      public async Task<HatchlingStateDto> ToDataTransferObjectAsync() {
         return new HatchlingStateDto {
            Id = Id,
            Name = Name,
            StartResult = await GetStartResultAsync()
         };
      }
   }
}