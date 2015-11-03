using Caliburn.Micro;
using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Core.Menus
{
    public class CommandMenuItemDefinition<T> : MenuItemDefinition where T : CommandDefinitionBase
    {
        readonly CommandDefinitionBase _commandDefinition;

        public override string Text { get { return _commandDefinition.Text; } }
        public override Uri IconSource { get { return _commandDefinition.IconSource; } }
        public override KeyGesture KeyGesture { get { return _commandDefinition.KeyGesture; } }
        public override CommandDefinitionBase CommandDefinition { get { return _commandDefinition; } }

        public CommandMenuItemDefinition(MenuItemGroupDefinition group, int sortOrder)
            : base(group, sortOrder)
        {
            _commandDefinition = IoC.Get<ICommandService>().GetCommandDefinition(typeof(T));
        }

        public CommandMenuItemDefinition(T commandDefinition, MenuItemGroupDefinition group, int sortOrder)
            : base(group, sortOrder)
        {
            _commandDefinition = commandDefinition;
        }
    }
}
