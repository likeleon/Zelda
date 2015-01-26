using Caliburn.Micro;
using System.Windows;
using Zelda.Editor.Modules.StatusBar.ViewModels;

namespace Zelda.Editor.Modules.StatusBar
{
    public interface IStatusBar
    {
        IObservableCollection<StatusBarItemViewModel> Items { get; }

        void AddItem(string message, GridLength width);
    }
}
