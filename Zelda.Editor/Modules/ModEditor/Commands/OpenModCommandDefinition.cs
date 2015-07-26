using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ModEditor.Commands
{
    [CommandDefinition]
    class OpenModCommandDefinition : CommandDefinition
    {
        public const string CommandName = "File.OpenMod";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Open mod..."; } }
        public override string ToolTip { get { return "Open"; } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.O, ModifierKeys.Control); } }
    }
}
