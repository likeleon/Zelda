using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Zelda.Editor.Modules.StatusBar.ViewModels;

namespace Zelda.Editor.Modules.StatusBar.Views
{
    public partial class StatusBarView : UserControl
    {
        private Grid _statusBarGrid;

        public StatusBarView()
        {
            InitializeComponent();
        }

        private void OnStatusBarGridLoaded(object sender, RoutedEventArgs e)
        {
            _statusBarGrid = sender as Grid;
            RefreshGridColumns();
        }

        private void RefreshGridColumns()
        {
            _statusBarGrid.ColumnDefinitions.Clear();
            foreach (StatusBarItemViewModel item in StatusBar.Items.Cast<StatusBarItemViewModel>())
                _statusBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = item.Width });
        }
    }
}
