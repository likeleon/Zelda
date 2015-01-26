using Caliburn.Micro;
using System.Windows;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.StatusBar.ViewModels
{
    public class StatusBarItemViewModel : PropertyChangedBase
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            internal set { this.SetProperty(ref _index, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { this.SetProperty(ref _message, value); }
        }

        private readonly GridLength _width;
        public GridLength Width
        {
            get { return _width; }
        }

        public StatusBarItemViewModel(string message, GridLength width)
        {
            _message = message;
            _width = width;
        }
    }
}
