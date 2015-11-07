using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.StringsEditor.Commands
{
    [CommandDefinition]
    class SetStringKeyCommandDefinition : CommandDefinition
    {
        public const string CommandName = "StringsEditor.SetStringKey";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "Change key..."; } }
        public override string ToolTip { get { return "Change key"; } }
        public override Uri IconSource { get { return "icon_rename.png".ToIconUri(); } }
        public override KeyGesture KeyGesture { get { return new KeyGesture(Key.F2); } }
    }
}
