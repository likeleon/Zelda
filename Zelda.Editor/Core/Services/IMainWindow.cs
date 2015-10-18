using System.Windows;
using System.Windows.Media;

namespace Zelda.Editor.Core.Services
{
    interface IMainWindow
    {
        WindowState WindowState { get; set; }
        double Width { get; set; }
        double Height { get; set; }

        string Title { get; set; }
        ImageSource Icon { get; set; }

        IShell Shell { get; }
    }
}
