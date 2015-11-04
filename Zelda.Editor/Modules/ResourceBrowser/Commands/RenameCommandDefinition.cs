using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class RenameCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ResourceBrowser.Rename";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "Rename..."; } }
        public override string ToolTip { get { return "Rename"; } }
        public override Uri IconSource { get { return "icon_rename.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.F2); } }
    }
}
