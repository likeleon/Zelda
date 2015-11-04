using System.Windows.Controls;
using Zelda.Editor.Modules.DialogsEditor.ViewModels;

namespace Zelda.Editor.Modules.DialogsEditor.Views
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
        }

        void dialogsTreeListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var editor = DataContext as EditorViewModel;
            if (!editor.BuildContextMenu())
                e.Handled = true; // display no context menu
        }
    }
}
