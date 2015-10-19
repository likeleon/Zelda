using Caliburn.Micro;
using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Core.ToolBars
{
    class CommandToolBarItemDefinition<TCommandDefinition> : ToolBarItemDefinition
        where TCommandDefinition : CommandDefinitionBase
    {
        readonly CommandDefinitionBase _commandDefinition;

        public override string Text { get { return _commandDefinition.ToolTip; } }
        public override Uri IconSource { get { return _commandDefinition.IconSource; } }
        public override KeyGesture KeyGesture { get { return _commandDefinition.KeyGesture; } }
        public override CommandDefinitionBase CommandDefinition { get { return _commandDefinition; } }

        public CommandToolBarItemDefinition(ToolBarItemGroupDefinition group, int sortOrder, ToolBarItemDisplay display = ToolBarItemDisplay.IconOnly)
            : base(group, sortOrder, display)
        {
            _commandDefinition = IoC.Get<ICommandService>().GetCommandDefinition(typeof(TCommandDefinition));
        }
    }
}
