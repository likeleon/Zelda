using System;
using System.Windows.Input;

namespace Zelda.Editor.Core.Commands
{
    abstract class CommandListDefinition : CommandDefinitionBase
    {
        public override sealed string Text { get { return "[NotUsed]"; } }
        public override sealed string ToolTip { get { return "[NotUsed]"; } }
        public override sealed Uri IconSource { get { return null; } }
        public override sealed KeyGesture KeyGesture { get { return null; } }
        public override sealed bool IsList { get { return true; } }
    }
}
