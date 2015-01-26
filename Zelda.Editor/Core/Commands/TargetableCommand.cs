using Caliburn.Micro;
using System;
using System.Windows.Input;

namespace Zelda.Editor.Core.Commands
{
    public class TargetableCommand : ICommand
    {
        private readonly Command _command;
        private readonly ICommandRouter _commandRouter;

        public TargetableCommand(Command command)
        {
            _command = command;
            _commandRouter = IoC.Get<ICommandRouter>();
        }

        public bool CanExecute(object parameter)
        {
            CommandHandlerWrapper commandHandler = _commandRouter.GetCommandHandler(_command.CommandDefinition);
            if (commandHandler == null)
                return false;

            commandHandler.Update(_command);

            return _command.Enabled;
        }

        public async void Execute(object parameter)
        {
            CommandHandlerWrapper commandHandler = _commandRouter.GetCommandHandler(_command.CommandDefinition);
            if (commandHandler == null)
                return;

            await commandHandler.Run(_command);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
