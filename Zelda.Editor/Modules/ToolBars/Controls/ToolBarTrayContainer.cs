using System.Windows;
using System.Windows.Controls;

namespace Zelda.Editor.Modules.ToolBars.Controls
{
    class ToolBarTrayContainer : ContentControl
    {
        static ToolBarTrayContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBarTrayContainer), new FrameworkPropertyMetadata(typeof(ToolBarTrayContainer)));
        }
    }
}
