using System;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Progress
{
    public class Job : IJob
    {
        private readonly string _name;
        public string Name
        {
            get { return _name; }
        }

        public int TotalWork
        {
            get;
            set;
        }

        public bool IsSystem
        {
            get;
            set;
        }

        public bool IsUser
        {
            get;
            set;
        }

        public Uri IconSource
        {
            get;
            set;
        }

        public Action<IProgressMonitor> Action
        {
            get { return _action; }
        }

        private readonly Action<IProgressMonitor> _action;

        public Job(string name, Action<IProgressMonitor> action)
        {
            if (name == null || action == null)
                throw new ArgumentNullException();

            _name = name;
            _action = action;

            TotalWork = 100;
            IsSystem = false;
            IsUser = false;
            IconSource = "BackgroundWorker_6235.png".ToIconUri();
        }
    }
}
