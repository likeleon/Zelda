using System.Windows.Controls.Primitives;

namespace Zelda.Editor.Modules.ToolBars.Controls
{
    class CustomToggleButton : ToggleButton
    {
        protected override void OnToggle()
        {
            // Don't update IsChecked - we'll do that with a binding.
        }
    }
}
