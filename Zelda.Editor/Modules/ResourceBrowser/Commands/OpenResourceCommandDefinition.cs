using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class OpenResourceCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.OpenResource";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Open"; } }
        public override string ToolTip { get { return "Open"; } }
    }
}
