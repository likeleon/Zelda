using System;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class DeleteCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.Dekete";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "Delete..."; } }
        public override string ToolTip { get { return "Delete"; } }
        public override Uri IconSource { get { return "icon_delete.png".ToIconUri(); } }
    }
}
