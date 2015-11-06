using System.Windows.Controls;
using System.Windows.Input;
using Zelda.Editor.Core.Controls;

namespace Zelda.Editor.Modules.ResourceBrowser.Views
{
    public partial class ResourceBrowserView : UserControl
    {
        IResourceBrowser Browser { get { return DataContext as IResourceBrowser; } }

        public ResourceBrowserView()
        {
            InitializeComponent();
        }

        void TreeListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Browser != null && !Browser.BuildContextMenu())
                e.Handled = true; // display no context menu
        }

        void TreeListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeListViewItem;
            if (item == null || !item.IsSelected)
                return;

            var modFile = item.DataContext as IModFile;
            if (modFile != null && Browser != null)
                Browser.Open(modFile);
        }
    }
}
