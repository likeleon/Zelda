using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.UndoRedo.Commands
{
    [CommandDefinition]
    class RedoCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Edit.Redo";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Redo"; } }
        public override string ToolTip { get { return "Redo"; } }
        public override Uri IconSource { get { return "Redo.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.Y, ModifierKeys.Control); } }
    }
}
