using Caliburn.Micro;
using System.Windows.Input;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.ToolBars;
using Zelda.Editor.Modules.ToolBars;
using Zelda.Editor.Modules.ToolBars.Models;

namespace Zelda.Editor.Core
{
    abstract class Tool : LayoutItemBase, ITool
    {
        bool _isVisible;
        ICommand _closeCommand;
        ToolBarDefinition _toolBarDefinition;
        IToolBar _toolBar;

        public override ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => IsVisible = false, p => true)); }
        }

        public abstract PaneLocation PreferredLocation { get; }
        public virtual double PreferredWidth { get { return 200; } }
        public virtual double PreferredHeight { get { return 200; } }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { this.SetProperty(ref _isVisible, value); }
        }

        public ToolBarDefinition ToolBarDefinition
        {
            get { return _toolBarDefinition; }
            protected set
            {
                _toolBarDefinition = value;
                NotifyOfPropertyChange(() => ToolBar);
                NotifyOfPropertyChange();
            }
        }

        public IToolBar ToolBar
        {
            get
            {
                if (_toolBar != null)
                    return _toolBar;

                if (ToolBarDefinition == null)
                    return null;

                var toolBarBuilder = IoC.Get<IToolBarBuilder>();
                _toolBar = new ToolBarModel();
                toolBarBuilder.BuildToolBar(_toolBarDefinition, _toolBar);
                return _toolBar;
            }
        }

        protected Tool()
        {
            IsVisible = true;
        }
    }
}
