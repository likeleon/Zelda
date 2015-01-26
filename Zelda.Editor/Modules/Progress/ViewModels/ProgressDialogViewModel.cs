using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.Progress.ViewModels
{
    public class ProgressDialogViewModel : WindowBase
    {
        public JobViewModel JobItem
        {
            get;
            private set;
        }

        public RelayCommand RunInBackgroundCommand
        {
            get;
            private set;
        }

        public RelayCommand OkCommand
        {
            get;
            private set;
        }

        private Task _jobTask;

        public ProgressDialogViewModel(JobViewModel jobItem)
        {
            JobItem = jobItem;
            JobItem.IsRunningChanged += OnJobItemIsRunningChanged;

            RunInBackgroundCommand = new RelayCommand(o => TryClose(false), o => JobItem.IsRunning);
            OkCommand = new RelayCommand(o => TryClose(true), o => !JobItem.IsRunning);
            DisplayName = jobItem.Job.Name;
            
            _jobTask = RunJob();
        }

        private void OnJobItemIsRunningChanged(object sender, EventArgs e)
        {
            RunInBackgroundCommand.RaiseCanExecuteChanged();
            OkCommand.RaiseCanExecuteChanged();
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                JobItem.IsRunningChanged -= OnJobItemIsRunningChanged;

            base.OnDeactivate(close);
        }

        private async Task RunJob()
        {
            await JobItem.Run();
        }
    }
}
