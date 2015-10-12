using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandDefinition]
    class SaveFileCommandDefinition : CommandDefinition
    {
        public const string CommandName = "File.SaveFile";

        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Save"; } }
        public override string ToolTip { get { return "Save"; } }
        public override Uri IconSource { get { return "Save.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.S, ModifierKeys.Control); } }
    }
}
