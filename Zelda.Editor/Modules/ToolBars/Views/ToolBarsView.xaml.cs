using System.Windows.Controls;

namespace Zelda.Editor.Modules.ToolBars.Views
{
    /// <summary>
    /// Interaction logic for ToolBarsView.xaml
    /// </summary>
    public partial class ToolBarsView : UserControl, IToolBarsView
    {
        public ToolBarsView()
        {
            InitializeComponent();
        }

        ToolBarTray IToolBarsView.ToolBarTray
        {
            get { return toolBarTray; }
        }
    }
}
