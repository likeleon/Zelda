
namespace Zelda.Editor.Core.Commands
{
    public interface ICommandRouter
    {
        CommandHandlerWrapper GetCommandHandler(CommandDefinitionBase commandDefinition);
    }
}
