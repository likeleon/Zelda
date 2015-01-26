using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.MainWindow.ViewModels
{
    [Export(typeof(IMainWindow))]
    public class MainWindowViewModel : Conductor<IShell>, IMainWindow
    {
#pragma warning disable 649
        [Import]
        private IShell _shell;

        [Import]
        private ICommandKeyGestureService _commandKeyGestureService;
#pragma warning restore 649

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get { return _windowState; }
            set { this.SetProperty(ref _windowState, value); }
        }

        private double _width = 1000.0;
        public double Width
        {
            get { return _width; }
            set { this.SetProperty(ref _width, value); }
        }

        private double _height = 800.0;
        public double Height
        {
            get { return _height; }
            set { this.SetProperty(ref _height, value); }
        }

        private string _title = "[Default Title]";
        public string Title
        {
            get { return _title; }
            set { this.SetProperty(ref _title, value); }
        }

        private ImageSource _icon;
        public ImageSource Icon
        {
            get { return _icon; }
            set { this.SetProperty(ref _icon, value); }
        }

        public IShell Shell
        {
            get { return _shell; }
        }

        protected override void OnViewLoaded(object view)
        {
            _commandKeyGestureService.BindKeyGestures(view as UIElement);
            base.OnViewLoaded(view);
        }
    }
}
