using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.Progress.Commands;

namespace Zelda.Editor.Modules.Progress
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition ViewProgressMenuItem = new CommandMenuItemDefinition<ViewProgressCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 2);
    }
}
