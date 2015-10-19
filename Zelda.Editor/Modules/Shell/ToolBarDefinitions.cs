using System.ComponentModel.Composition;
using Zelda.Editor.Core.ToolBars;
using Zelda.Editor.Modules.Shell.Commands;

namespace Zelda.Editor.Modules.Shell
{
    static class ToolBarDefinitions
    {
        [Export]
        public static ToolBarItemGroupDefinition StandardOpenSaveToolBarGroup = new ToolBarItemGroupDefinition(
            ToolBars.ToolBarDefinitions.StandardToolBar, 8);

        [Export]
        public static ToolBarItemDefinition SaveFileToolBarItem = new CommandToolBarItemDefinition<SaveFileCommandDefinition>(
            StandardOpenSaveToolBarGroup, 2);
    }
}
