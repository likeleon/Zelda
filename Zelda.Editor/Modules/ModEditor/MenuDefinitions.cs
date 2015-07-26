using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.ModEditor.Commands;

namespace Zelda.Editor.Modules.ModEditor
{
    public static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition OpenModMenuItem = new CommandMenuItemDefinition<OpenModCommandDefinition>(MainMenu.MenuDefinitions.FileNewOpenMenuGroup, 0);
    }
}
