using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Zelda.Editor.Core.Commands
{
    [Export(typeof(ICommandService))]
    public class CommandService : ICommandService
    {
        readonly Dictionary<Type, CommandDefinitionBase> _commandDefinitionsLookup = new Dictionary<Type, CommandDefinitionBase>();
        readonly Dictionary<CommandDefinitionBase, Command> _commands = new Dictionary<CommandDefinitionBase, Command>();
        readonly Dictionary<Command, TargetableCommand> _targetableCommands = new Dictionary<Command, TargetableCommand>();
        
        readonly CommandDefinitionBase[] _commandDefinitions;

        [ImportingConstructor]
        public CommandService([ImportMany] CommandDefinitionBase[] commandDefinitions)
        {
            _commandDefinitions = commandDefinitions;
        }

        public CommandDefinitionBase GetCommandDefinition(Type commandDefinitionType)
        {
            CommandDefinitionBase commandDefinition;
            if (!_commandDefinitionsLookup.TryGetValue(commandDefinitionType, out commandDefinition))
                commandDefinition = _commandDefinitionsLookup[commandDefinitionType] =
                    _commandDefinitions.First(x => x.GetType() == commandDefinitionType);
            return commandDefinition;
        }

        public Command GetCommand(CommandDefinitionBase commandDefinition)
        {
            Command command;
            if (!_commands.TryGetValue(commandDefinition, out command))
                command = _commands[commandDefinition] = new Command(commandDefinition);
            return command;
        }

        public TargetableCommand GetTargetableCommand(Command command)
        {
            TargetableCommand targetableCommand;
            if (!_targetableCommands.TryGetValue(command, out targetableCommand))
                targetableCommand = _targetableCommands[command] = new TargetableCommand(command);
            return targetableCommand;
        }
    }
}
