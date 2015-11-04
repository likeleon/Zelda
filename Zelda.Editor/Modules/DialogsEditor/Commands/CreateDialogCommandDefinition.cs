using System;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.DialogsEditor.Commands
{
    [CommandDefinition]
    class CreateDialogCommandDefinition : CommandDefinition
    {
        public const string CommandName = "DialogsEditor.CreateDialog";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "New dialog..."; } }
        public override string ToolTip { get { return "New dialog"; } }
        public override Uri IconSource { get { return "icon_add.png".ToIconUri(); } }
    }
}
