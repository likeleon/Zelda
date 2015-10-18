namespace Zelda.Editor.Core.Commands
{
    interface ICommandUiItem
    {
        CommandDefinitionBase CommandDefinition { get; }
        void Update(CommandHandlerWrapper commandHandler);
    }
}
