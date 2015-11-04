using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class OpenMapScriptCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.OpenMapScript";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Open Script"; } }
        public override string ToolTip { get { return "Open Script"; } }
    }
}
