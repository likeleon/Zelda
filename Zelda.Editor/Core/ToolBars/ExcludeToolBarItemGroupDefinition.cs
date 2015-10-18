namespace Zelda.Editor.Core.ToolBars
{
    class ExcludeToolBarItemGroupDefinition
    {
        public ToolBarItemGroupDefinition ToolBarItemGroupDefinitionToExclude { get; private set; }

        public ExcludeToolBarItemGroupDefinition(ToolBarItemGroupDefinition toolBarItemGroupDefinition)
        {
            ToolBarItemGroupDefinitionToExclude = toolBarItemGroupDefinition;
        }
    }
}
