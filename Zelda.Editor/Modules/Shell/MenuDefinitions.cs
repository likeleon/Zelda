using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.Shell.Commands;

namespace Zelda.Editor.Modules.Shell
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition FileExitMenuItem = new CommandMenuItemDefinition<ExitCommandDefinition>(MainMenu.MenuDefinitions.FileExitOpenMenuGroup, 0);
    }
}
