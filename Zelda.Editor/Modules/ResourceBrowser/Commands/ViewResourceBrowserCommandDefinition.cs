using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    public class ViewResourceBrowserCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.ResourceBrowser";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Resource Browser"; } }
        public override string ToolTip { get { return "Resource Browser"; } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Alt); } }
    }
}
