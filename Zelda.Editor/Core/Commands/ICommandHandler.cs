using System.Threading.Tasks;

namespace Zelda.Editor.Core.Commands
{
    public interface ICommandHandler<T> : ICommandHandler where T : CommandDefinition
    {
        void Update(Command command);
        Task Run(Command command);
    }

    public interface ICommandHandler
    {
    }

    public abstract class CommandHandlerBase<T> : ICommandHandler<T> where T : CommandDefinition
    {
        public virtual void Update(Command command)
        {
        }

        public abstract Task Run(Command command);
    }
}
