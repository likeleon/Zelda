using Caliburn.Micro;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Progress.Commands
{
    [CommandHandler]
    class ViewProgressCommandHandler : CommandHandlerBase<ViewProgressCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewProgressCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            _shell.ShowTool<IProgressTool>();
            return ScheduleNewJob();
        }

        private Task ScheduleNewJob()
        {
            int targetSleepSeconds = 10;
            Job job = new Job("My Job Name", monitor =>
            {
                int sleptSeconds = 0;
                monitor.Worked("Starting...", sleptSeconds);

                while (sleptSeconds < targetSleepSeconds)
                {
                    //monitor.ThrowIfCancellationRequested();

                    // 실패 처리 테스트
                    if (monitor.IsCanceled)
                        throw new InvalidOperationException();

                    int sleepUnit = 3;
                    Thread.Sleep(sleepUnit * 1000);
                    sleptSeconds += sleepUnit;
                    monitor.Worked(string.Format("Slepted {0} seconds", sleptSeconds), sleepUnit); 
                }
            });
            job.TotalWork = targetSleepSeconds;
            job.IsUser = true;

            IProgressService progress = IoC.Get<IProgressService>();
            return progress.Run(job);
        }
    }
}
