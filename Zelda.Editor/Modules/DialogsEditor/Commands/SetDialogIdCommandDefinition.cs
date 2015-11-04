using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.DialogsEditor.Commands
{
    [CommandDefinition]
    class SetDialogIdCommandDefinition : CommandDefinition
    {
        public const string CommandName = "DialogsEditor.SetDialogId";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "Change id..."; } }
        public override string ToolTip { get { return "Change id"; } }
        public override Uri IconSource { get { return "icon_rename.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.F2); } }
    }
}
