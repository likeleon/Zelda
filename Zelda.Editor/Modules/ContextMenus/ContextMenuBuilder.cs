using System.ComponentModel.Composition;
using System.Linq;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Modules.MainMenu.Models;

namespace Zelda.Editor.Modules.ContextMenus
{
    [Export(typeof(IContextMenuBuilder))]
    class ContextMenuBuilder : IContextMenuBuilder
    {
        readonly ICommandService _commandService;

        [ImportingConstructor]
        public ContextMenuBuilder(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public void BuildContextMenu(MenuItemDefinition[] menuItemDefinitions, IContextMenu contextMenu)
        {
            var menuItemsByGroup = menuItemDefinitions
                .GroupBy(x => x.Group)
                .OrderBy(x => x.Key.SortOrder);

            var i = 0;
            foreach (var data in menuItemsByGroup)
            {
                var group = data.Key;
                var menuItems = menuItemDefinitions
                    .Where(x => x.Group == group)
                    .OrderBy(x => x.SortOrder);

                foreach (var menuItem in menuItems)
                {
                    var menuItemModel = (menuItem.CommandDefinition != null)
                        ? new CommandMenuItem(_commandService.GetCommand(menuItem.CommandDefinition), null)
                        : new TextMenuItem(menuItem) as StandardMenuItem;
                    contextMenu.Add(menuItemModel);
                }

                if (i < menuItemsByGroup.Count() - 1 && menuItems.Any())
                    contextMenu.Add(new MenuItemSeparator());
                ++i;
            }
        }
    }
}
