using Caliburn.Micro;
using System.ComponentModel.Composition;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.ToolBars.Controls;
using Zelda.Editor.Modules.ToolBars.Views;

namespace Zelda.Editor.Modules.ToolBars.ViewModels
{
    [Export(typeof(IToolBars))]
    class ToolBarsViewModel : ViewAware, IToolBars
    {
        readonly BindableCollection<IToolBar> _items = new BindableCollection<IToolBar>();
        readonly IToolBarBuilder _toolBarBuilder;
        bool _visible;

        public IObservableCollection<IToolBar> Items { get { return _items; } }

        public bool Visible
        {
            get { return _visible; }
            set { this.SetProperty(ref _visible, value); }
        }

        [ImportingConstructor]
        public ToolBarsViewModel(IToolBarBuilder toolBarBuilder)
        {
            _toolBarBuilder = toolBarBuilder;
        }

        protected override void OnViewLoaded(object view)
        {
            _toolBarBuilder.BuildToolBars(this);

            foreach (var toolBar in Items)
            {
                var mainToolBar = new MainToolBar() { ItemsSource = toolBar };
                ((IToolBarsView)view).ToolBarTray.ToolBars.Add(mainToolBar);
            }

            base.OnViewLoaded(view);
        }
    }
}
