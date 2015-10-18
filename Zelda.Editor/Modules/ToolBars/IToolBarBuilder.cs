using Zelda.Editor.Core.ToolBars;

namespace Zelda.Editor.Modules.ToolBars
{
    interface IToolBarBuilder
    {
        void BuildToolBars(IToolBars result);
        void BuildToolBar(ToolBarDefinition toolBarDefinition, IToolBar result);
    }
}
