using System.Windows;
using System.Windows.Controls;

namespace Zelda.Editor.Modules.ToolBars.Controls
{
    class ToolPaneToolBar : ToolBarBase
    {
        static ToolPaneToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolPaneToolBar), new FrameworkPropertyMetadata(typeof(ToolPaneToolBar)));
        }

        public ToolPaneToolBar()
        {
            SetOverflowMode(this, OverflowMode.AsNeeded);
            ToolBarTray.SetIsLocked(this, true);
        }
    }
}
