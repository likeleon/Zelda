using System;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class NewDirectoryCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.NewDirectory";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "New folder..."; } }
        public override string ToolTip { get { return "New folder"; } }
        public override Uri IconSource { get { return "icon_folder_closed.png".ToIconUri(); } }
    }
}
