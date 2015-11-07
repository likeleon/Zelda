using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.StringsEditor.Commands
{
    [CommandDefinition]
    class DeleteStringCommandDefinition : CommandDefinition
    {
        public const string CommandName = "StringsEditor.DeleteString";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "Delete..."; } }
        public override string ToolTip { get { return "Delete"; } }
        public override Uri IconSource { get { return "icon_delete.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.Delete); } }
    }
}
