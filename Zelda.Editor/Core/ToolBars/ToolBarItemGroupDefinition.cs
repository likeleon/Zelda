namespace Zelda.Editor.Core.ToolBars
{
    class ToolBarItemGroupDefinition
    {
        public ToolBarDefinition ToolBar { get; private set; }
        public int SortOrder { get; private set; }

        public ToolBarItemGroupDefinition(ToolBarDefinition toolBar, int sortOrder)
        {
            ToolBar = toolBar;
            SortOrder = sortOrder;
        }
    }
}