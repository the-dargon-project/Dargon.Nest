using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Nest {
   public class UpdateNestOptions {
      public IReadOnlyList<string> ExcludedEggs { get; set; } = new List<string>();
      public UpdateState UpdateState { get; set; } = new UpdateState();
   }

   public class UpdateState : INotifyPropertyChanged {
      private string status;
      private double progress;
      private string subStatus;
      private double subProgress;
      public event PropertyChangedEventHandler PropertyChanged;

      public string Status { get { return status; } set { status = value; OnPropertyChanged(); } }
      //[0.0, 1.0], NaN
      public double Progress { get { return progress; } set { progress = value; OnPropertyChanged(); } }

      public string SubStatus { get { return subStatus; } set { subStatus = value; OnPropertyChanged(); } }
      //[0.0, 1.0], NaN
      public double SubProgress { get { return subProgress; } set { subProgress = value; OnPropertyChanged(); } }

      public void SetState(string status, double progress) {
         Status = status;
         Progress = progress;
      }

      public void SetState(string status, double progress, string subStatus, double subProgress) {
         Status = status;
         Progress = progress;
         SubStatus = subStatus;
         SubProgress = subProgress;
      }

      public void SetSubState(string subStatus, double subProgress) {
         SubStatus = subStatus;
         SubProgress = subProgress;
      }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }

   public enum UpdatePhase {
      PullingPackageList,
      UpdatingPackage
   }
}
