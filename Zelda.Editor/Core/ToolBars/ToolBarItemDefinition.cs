using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Core.ToolBars
{
    abstract class ToolBarItemDefinition
    {
        public ToolBarItemGroupDefinition Group { get; private set; }
        public int SortOrder { get; private set; }
        public ToolBarItemDisplay Display { get; private set; }

        public abstract string Text { get; }
        public abstract Uri IconSource { get; }
        public abstract KeyGesture KeyGesture { get; }
        public abstract CommandDefinitionBase CommandDefinition { get; }

        protected ToolBarItemDefinition(ToolBarItemGroupDefinition group, int sortOrder, ToolBarItemDisplay display)
        {
            Group = group;
            SortOrder = sortOrder;
            Display = display;
        }
    }

    enum ToolBarItemDisplay
    {
        IconOnly,
        IconAndText
    }
}