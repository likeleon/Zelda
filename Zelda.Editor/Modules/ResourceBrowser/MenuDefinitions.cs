using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.ResourceBrowser.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition ViewResourceBrowserMenuItem = new CommandMenuItemDefinition<ViewResourceBrowserCommandDefinition>(MainMenu.MenuDefinitions.ViewToolsMenuGroup, 1);
    }
}
