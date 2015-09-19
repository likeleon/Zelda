using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.UndoRedo.Commands;

namespace Zelda.Editor.Modules.UndoRedo
{
    static class MenuDefinitions
    {
        [Export]
        public static MenuItemDefinition EditUndoMenuItem = new CommandMenuItemDefinition<UndoCommandDefinition>(
            MainMenu.MenuDefinitions.EditUndoRedoMenuGroup, 0);

        [Export]
        public static MenuItemDefinition EditRedoMenuItem = new CommandMenuItemDefinition<RedoCommandDefinition>(
            MainMenu.MenuDefinitions.EditUndoRedoMenuGroup, 1);
    }
}
