using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class OpenLanguageStringsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.OpenLanguageStrings";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Open Strings"; } }
        public override string ToolTip { get { return "Open Strings"; } }
    }
}
