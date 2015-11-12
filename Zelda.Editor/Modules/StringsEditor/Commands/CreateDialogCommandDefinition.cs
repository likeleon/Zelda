using System;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.StringsEditor.Commands
{
    [CommandDefinition]
    class CreateStringCommandDefinition : CommandDefinition
    {
        public const string CommandName = "StringsEditor.CreateString";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "New string..."; } }
        public override string ToolTip { get { return "New string"; } }
        public override Uri IconSource { get { return "icon_add.png".ToIconUri(); } }
    }
}
