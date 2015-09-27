using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Output.Commands
{
    [CommandDefinition]
    public class ViewOutputCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Output";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Output"; } }
        public override string ToolTip { get { return "Output"; } }
        public override Uri IconSource { get { return "ImmediateWindow_2644.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Alt); } }
    }
}
