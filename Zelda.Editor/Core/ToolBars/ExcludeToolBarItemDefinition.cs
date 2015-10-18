namespace Zelda.Editor.Core.ToolBars
{
    class ExcludeToolBarItemDefinition
    {
        public ToolBarItemDefinition ToolBarItemDefinitionToExclude { get; private set; }

        public ExcludeToolBarItemDefinition(ToolBarItemDefinition toolBarItemDefinition)
        {
            ToolBarItemDefinitionToExclude = toolBarItemDefinition;
        }
    }
}
