using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Zelda.Editor.Core.Commands
{
    public sealed class CommandHandlerWrapper
    {
        readonly object _commandHandler;
        readonly MethodInfo _updateMethod;
        readonly MethodInfo _runMethod;

        public static CommandHandlerWrapper FromCommandHandler(Type commandHandlerInterfaceType, object commandHandler)
        {
            var updateMethod = commandHandlerInterfaceType.GetMethod("Update");
            var runMethod = commandHandlerInterfaceType.GetMethod("Run");
            return new CommandHandlerWrapper(commandHandler, updateMethod, runMethod);
        }
        

        CommandHandlerWrapper(object commandHandler, MethodInfo updateMethod, MethodInfo runMethod)
        {
            _commandHandler = commandHandler;
            _updateMethod = updateMethod;
            _runMethod = runMethod;
        }

        public void Update(Command command)
        {
            if (_updateMethod != null)
                _updateMethod.Invoke(_commandHandler, new object[] { command });
        }

        public Task Run(Command command)
        {
            return (Task)_runMethod.Invoke(_commandHandler, new object[] { command });
        }
    }
}
