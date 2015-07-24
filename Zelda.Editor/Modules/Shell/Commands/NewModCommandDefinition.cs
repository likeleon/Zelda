using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandDefinition]
    class NewModCommandDefinition : CommandDefinition
    {
        public const string CommandName = "File.NewMod";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_New mod..."; } }
        public override string ToolTip { get { return "New"; } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.O, ModifierKeys.Control); } }
    }
}
