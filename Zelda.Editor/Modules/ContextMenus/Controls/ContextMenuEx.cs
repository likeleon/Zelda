using System.Windows;
using System.Windows.Controls;
using Zelda.Editor.Modules.MainMenu.Controls;

namespace Zelda.Editor.Modules.ContextMenus.Controls
{
    class ContextMenuEx : ContextMenu
    {
        object _currentItem;

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            _currentItem = item;
            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return MenuItemEx.GetContainer(this, _currentItem);
        }
    }
}
