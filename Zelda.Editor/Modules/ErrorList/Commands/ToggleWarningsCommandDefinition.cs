using System;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ErrorList.Commands
{
    [CommandDefinition]
    class ToggleWarningsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ErrorList.ToggleWarnings";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "[NotUsed]"; } }
        public override string ToolTip { get { return "[NotUsed]"; } }
        public override Uri IconSource { get { return new Uri("/Modules/ErrorList/Resources/Warning.png", UriKind.Relative); } }
    }
}
