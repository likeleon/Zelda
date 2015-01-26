using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.ErrorList.Commands;

namespace Zelda.Editor.Modules.ErrorList
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition ViewOutputMenuItem = new CommandMenuItemDefinition<ViewErrorListCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 0);
    }
}
