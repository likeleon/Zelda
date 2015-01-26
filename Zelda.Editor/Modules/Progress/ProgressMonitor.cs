using System;
using System.Threading;

namespace Zelda.Editor.Modules.Progress
{
    public class ProgressMonitor : IProgressMonitor
    {
        public class ProgressData
        {
            public string Message { get; set; }
            public int TotalWorked { get; set; }
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private IProgress<ProgressData> _progress;
        private int _totalWorked;

        public bool IsCanceled
        {
            get { return _cts.IsCancellationRequested; }
        }

        public void ThrowIfCancellationRequested()
        {
            _cts.Token.ThrowIfCancellationRequested();
        }

        public void Cancel()
        {
            if (CanCancel())
                _cts.Cancel();
        }

        public bool CanCancel()
        {
            return _cts.Token.CanBeCanceled && !_cts.IsCancellationRequested;
        }

        // 지정한 양만큼의 작업이 완료되었음을 알림
        public void Worked(string message, int amount)
        {
            _totalWorked += amount;

            _progress.Report(new ProgressData()
            {
                Message = message,
                TotalWorked = _totalWorked
            });
        }

        public ProgressMonitor(Action<ProgressData> progressHandler, Action cancellationCallback)
        {
            if (progressHandler == null || cancellationCallback == null)
                throw new ArgumentNullException();

            _progress = new Progress<ProgressData>(progressHandler);
            _cts.Token.Register(cancellationCallback);
        }
    }
}
