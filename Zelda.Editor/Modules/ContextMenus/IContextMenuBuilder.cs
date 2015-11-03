using System.Collections.Generic;
using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.ContextMenus
{
    interface IContextMenuBuilder
    {
        void BuildContextMenu(IEnumerable<MenuItemDefinition> menuItems, IContextMenu contextMenu);
    }
}
