using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.MainMenu.Models;

namespace Zelda.Editor.Modules.MainMenu
{
    public interface IMenuBuilder
    {
        void BuildMenuBar(MenuBarDefinition menuBarDefinition, MenuModel result);
    }
}
