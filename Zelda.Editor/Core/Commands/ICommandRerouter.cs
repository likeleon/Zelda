
namespace Zelda.Editor.Core.Commands
{
    public interface ICommandRerouter
    {
        object GetHandler(CommandDefinitionBase commandDefinition);
    }
}
