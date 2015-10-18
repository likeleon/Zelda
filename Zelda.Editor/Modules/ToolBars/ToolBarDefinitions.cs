using System.ComponentModel.Composition;
using Zelda.Editor.Core.ToolBars;

namespace Zelda.Editor.Modules.ToolBars
{
    static class ToolBarDefinitions
    {
        [Export]
        public static ToolBarDefinition StandardToolBar = new ToolBarDefinition(0, "Standard");
    }
}
