using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.Output.Commands;

namespace Zelda.Editor.Modules.Output
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition ViewOutputMenuItem = new CommandMenuItemDefinition<ViewOutputCommandDefinition>(MainMenu.MenuDefinitions.ViewToolsMenuGroup, 1);
    }
}
