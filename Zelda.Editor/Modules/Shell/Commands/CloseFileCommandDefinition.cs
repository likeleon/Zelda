using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandDefinition]
    class CloseFileCommandDefinition : CommandDefinition
    {
        public const string CommandName = "File.CloseFile";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Close"; } }
        public override string ToolTip { get { return "Close"; } }
    }
}
