using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Windows;

namespace Zelda.Editor.Modules.StatusBar.ViewModels
{
    [Export(typeof(IStatusBar))]
    public class StatusBarViewModel : PropertyChangedBase, IStatusBar
    {
        private readonly StatusBarItemCollection _items = new StatusBarItemCollection();
        public IObservableCollection<StatusBarItemViewModel> Items
        {
            get { return _items; }
        }

        public void AddItem(string message, GridLength width)
        {
            Items.Add(new StatusBarItemViewModel(message, width));
        }

        private class StatusBarItemCollection : BindableCollection<StatusBarItemViewModel>
        {
            protected override void InsertItemBase(int index, StatusBarItemViewModel item)
            {
                item.Index = index;
                base.InsertItemBase(index, item);
            }

            protected override void SetItemBase(int index, StatusBarItemViewModel item)
            {
                item.Index = index;
                base.SetItemBase(index, item);
            }
        }
    }
}
