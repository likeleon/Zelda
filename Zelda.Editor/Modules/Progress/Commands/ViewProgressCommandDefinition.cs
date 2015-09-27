using System;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Progress.Commands
{
    [CommandDefinition]
    public class ViewProgressCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Progress";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Progress"; } }
        public override string ToolTip { get { return "Progress"; } }
        public override Uri IconSource { get { return "KPI_32xLG.png".ToIconUri(); } }
    }
}
