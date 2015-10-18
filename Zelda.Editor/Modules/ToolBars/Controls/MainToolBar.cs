using System.Windows.Controls;

namespace Zelda.Editor.Modules.ToolBars.Controls
{
    class MainToolBar : ToolBarBase
    {
        public MainToolBar()
        {
            SetOverflowMode(this, OverflowMode.Always);
            SetResourceReference(StyleProperty, typeof(ToolBar));
        }
    }
}
