using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.DialogsEditor
{
    static class ContextMenuDefinitions
    {
        [Export]
        public static MenuItemGroupDefinition CreateMenuGroup = new MenuItemGroupDefinition(null, 0);

        [Export]
        public static MenuItemGroupDefinition SetIdMenuGroup = new MenuItemGroupDefinition(null, 1);

        [Export]
        public static MenuItemGroupDefinition DeleteMenuGroup = new MenuItemGroupDefinition(null, 2);
    }
}
