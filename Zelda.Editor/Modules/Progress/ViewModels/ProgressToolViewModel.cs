using Caliburn.Micro;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Progress.ViewModels
{
    [Export(typeof(IProgressService))]
    [Export(typeof(IProgressTool))]
    public class ProgressToolViewModel : Tool, IProgressService, IProgressTool
    {
        public IObservableCollection<JobViewModel> Items
        {
            get { return _items; }
        }

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        private readonly IObservableCollection<JobViewModel> _items = new BindableCollection<JobViewModel>();
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public ProgressToolViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            DisplayName = "Progress";
        }

        public async Task Run(IJob job)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            JobViewModel item = new JobViewModel(job);
            _items.Add(item);

            if (job.IsUser)
                _windowManager.ShowDialog(new ProgressDialogViewModel(item));
            else
                await item.Run();
        }
    }
}
