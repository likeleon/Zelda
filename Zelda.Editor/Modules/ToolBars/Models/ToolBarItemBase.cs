using Caliburn.Micro;

namespace Zelda.Editor.Modules.ToolBars.Models
{
    class ToolBarItemBase : PropertyChangedBase
    {
        public static ToolBarItemBase Separator { get { return new ToolBarItemSeparator(); } }
        public virtual string Name { get { return "-"; } }
    }
}
