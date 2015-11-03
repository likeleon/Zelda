using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.ContextMenus
{
    interface IContextMenuBuilder
    {
        void BuildContextMenu(MenuItemDefinition[] menuItems, IContextMenu contextMenu);
    }
}
