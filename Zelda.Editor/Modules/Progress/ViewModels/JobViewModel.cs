using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.Progress.ViewModels
{
    public class JobViewModel : PropertyChangedBase
    {
        public event EventHandler IsRunningChanged;

        private readonly IJob _job;
        private readonly ProgressMonitor _progressMonitor;

        public IJob Job
        {
            get { return _job; }
        }

        public RelayCommand CancelCommand
        {
            get;
            private set;
        }

        private int _workProgress;
        public int WorkProgress
        {
            get { return _workProgress; }
            set { this.SetProperty(ref _workProgress, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { this.SetProperty(ref _message, value); }
        }

        public JobViewModel(IJob job)
        {
            _job = job;
            _progressMonitor = new ProgressMonitor(ProgressHandler, CancellationCallback);

            CancelCommand = new RelayCommand(
                obj => _progressMonitor.Cancel(),
                obj => IsRunning && _progressMonitor.CanCancel());
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            private set 
            { 
                if (this.SetProperty(ref _isRunning, value))
                {
                    if (IsRunningChanged != null)
                        IsRunningChanged(this, EventArgs.Empty);
                    
                    CancelCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get { return _isCompleted; }
            private set { this.SetProperty(ref _isCompleted, value); }
        }

        private bool _isFaulted;
        public bool IsFaulted
        {
            get { return _isFaulted; }
            private set { this.SetProperty(ref _isFaulted, value); }
        }

        private bool _isCanceled;
        public bool IsCanceled
        {
            get { return _isCanceled; }
            private set { this.SetProperty(ref _isCanceled, value); }
        }

        private Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
            private set 
            {
                if (this.SetProperty(ref _exception, value))
                    NotifyOfPropertyChange(() => HasException);
            }
        }

        public bool HasException
        {
            get { return _exception != null; }
        }

        private bool _isCancellationRequested;
        public bool IsCancellationRequested
        {
            get { return _isCancellationRequested; }
            set { this.SetProperty(ref _isCancellationRequested, value); }
        }

        public async Task Run()
        {
            IsRunning = true;
            try
            {
                await Task.Run(() => _job.Action(_progressMonitor));
                IsCompleted = true;
            }
            catch (OperationCanceledException ex)
            {
                IsCanceled = true;
                Exception = ex;
            }
            catch (Exception ex)
            {
                IsFaulted = true;
                Exception = ex;
            }
            IsRunning = false;
        }

        private void ProgressHandler(ProgressMonitor.ProgressData progress)
        {
            Message = progress.Message;
            WorkProgress = Math.Min(progress.TotalWorked, _job.TotalWork);
        }

        private void CancellationCallback()
        {
            IsCancellationRequested = true;
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
