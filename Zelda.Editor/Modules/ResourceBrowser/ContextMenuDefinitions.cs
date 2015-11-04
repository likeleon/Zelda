using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    static class ContextMenuDefinitions
    {
        [Export]
        public static MenuItemGroupDefinition OpenMenuGroup = new MenuItemGroupDefinition(null, 0);

        [Export]
        public static MenuItemGroupDefinition NewMenuGroup = new MenuItemGroupDefinition(null, 1);

        [Export]
        public static MenuItemGroupDefinition RenameMenuGroup = new MenuItemGroupDefinition(null, 2);

        [Export]
        public static MenuItemGroupDefinition DeleteMenuGroup = new MenuItemGroupDefinition(null, 3);
    }
}
