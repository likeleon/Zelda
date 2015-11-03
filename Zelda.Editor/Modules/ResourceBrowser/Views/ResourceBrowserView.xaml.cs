using System.Windows.Controls;
using Zelda.Editor.Modules.ResourceBrowser.ViewModels;

namespace Zelda.Editor.Modules.ResourceBrowser.Views
{
    public partial class ResourceBrowserView : UserControl
    {
        public ResourceBrowserView()
        {
            InitializeComponent();
        }

        void TreeListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var browser = DataContext as ResourceBrowserViewModel;
            if (!browser.BuildContextMenu())
                e.Handled = true; // display no context menu
        }
    }
}
