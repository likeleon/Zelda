using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.UndoRedo.Commands
{
    [CommandDefinition]
    class UndoCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Edit.Undo";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Undo"; } }
        public override string ToolTip { get { return "Undo"; } }
        public override Uri IconSource { get { return "/Resources/Icons/Undo.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.Z, ModifierKeys.Control); } }
    }
}
