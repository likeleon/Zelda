using System;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class ChangeDescriptionCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.ChangeDescription";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "Change description..."; } }
        public override string ToolTip { get { return "Change description"; } }
    }
}
