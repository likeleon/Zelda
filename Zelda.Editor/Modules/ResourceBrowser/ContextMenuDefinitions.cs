using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    static class ContextMenuDefinitions
    {
        [Export]
        public static MenuItemGroupDefinition OpenMenuGroup = new MenuItemGroupDefinition(null, 0);
    }
}
