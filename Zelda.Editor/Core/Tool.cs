using System.Windows.Input;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Core
{
    public abstract class Tool : LayoutItemBase, ITool
    {
        private ICommand _closeCommand;
        public override ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => IsVisible = false, p => true)); }
        }

        public abstract PaneLocation PreferredLocation { get; }

        public virtual double PreferredWidth
        {
            get { return 200; }
        }

        public virtual double PreferredHeight
        {
            get { return 200; }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { this.SetProperty(ref _isVisible, value); }
        }

        protected Tool()
        {
            IsVisible = true;
        }
    }
}
