namespace Zelda.Editor.Core.ToolBars
{
    class ToolBarDefinition
    {
        readonly int _sortOrder;
        readonly string _name;

        public int SortOrder { get { return _sortOrder; } }
        public string Name { get { return _name; } }

        public ToolBarDefinition(int sortOrder, string name)
        {
            _sortOrder = sortOrder;
            _name = name;
        }
    }
}
