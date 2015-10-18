namespace Zelda.Editor.Core.ToolBars
{
    class ExcludeToolBarDefinition
    {
        public ToolBarDefinition ToolBarDefinitionToExclude { get; private set; }

        public ExcludeToolBarDefinition(ToolBarDefinition toolBarDefinition)
        {
            ToolBarDefinitionToExclude = toolBarDefinition;
        }
    }
}
