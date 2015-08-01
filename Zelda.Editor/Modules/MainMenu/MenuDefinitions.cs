using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.MainMenu
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuBarDefinition MainMenuBar = new MenuBarDefinition();

        [Export]
        public static MenuDefinition FileMenu = new MenuDefinition(MainMenuBar, 0, "_File");

        [Export]
        public static MenuItemGroupDefinition FileNewOpenMenuGroup = new MenuItemGroupDefinition(FileMenu, 0);

        [Export]
        public static MenuItemGroupDefinition FileCloseMenuGroup = new MenuItemGroupDefinition(FileMenu, 3);

        [Export]
        public static MenuItemGroupDefinition FileExitOpenMenuGroup = new MenuItemGroupDefinition(FileMenu, 10);

        [Export]
        public static MenuDefinition ViewMenu = new MenuDefinition(MainMenuBar, 2, "_View");

        [Export]
        public static MenuItemGroupDefinition ViewToolsMenuGroup = new MenuItemGroupDefinition(ViewMenu, 0);
    }
}
