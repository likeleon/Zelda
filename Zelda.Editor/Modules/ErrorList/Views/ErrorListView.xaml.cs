using System.Windows.Controls;
using System.Windows.Input;

namespace Zelda.Editor.Modules.ErrorList.Views
{
    /// <summary>
    /// Interaction logic for ErrorListView.xaml
    /// </summary>
    public partial class ErrorListView : UserControl
    {
        public ErrorListView()
        {
            InitializeComponent();
        }

        void OnDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count != 1)
                return;

            var item = dataGrid.SelectedItem as ErrorListItem;
            if (item.OnClick != null)
                item.OnClick();
        }
    }
}
