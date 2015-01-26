using System.ComponentModel.Composition;
using Zelda.Editor.Core.Menus;
using System.Linq;
using Zelda.Editor.Modules.MainMenu.Models;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.MainMenu
{
    [Export(typeof(IMenuBuilder))]
    public class MenuBuilder : IMenuBuilder
    {
        private readonly ICommandService _commandService;
        private readonly MenuBarDefinition[] _menuBars;
        private readonly MenuDefinition[] _menus;
        private readonly MenuItemGroupDefinition[] _menuItemGroups;
        private readonly MenuItemDefinition[] _menuItems;

        [ImportingConstructor]
        public MenuBuilder(
            ICommandService commandService,
            [ImportMany] MenuBarDefinition[] menuBars,
            [ImportMany] MenuDefinition[] menus,
            [ImportMany] MenuItemGroupDefinition[] menuItemGroups,
            [ImportMany] MenuItemDefinition[] menuItems)
        {
            _commandService = commandService;
            _menuBars = menuBars;
            _menus = menus;
            _menuItemGroups = menuItemGroups;
            _menuItems = menuItems;
        }

        public void BuildMenuBar(MenuBarDefinition menuBarDefinition, MenuModel result)
        {
            var menus = _menus
                .Where(x => x.MenuBar == menuBarDefinition)
                .OrderBy(x => x.SortOrder);

            foreach (MenuDefinition menu in menus)
            {
                TextMenuItem menuModel = new TextMenuItem(menu);
                AddGroupsRecursive(menu, menuModel);
                if (menuModel.Children.Any())
                    result.Add(menuModel);
            }
        }

        private void AddGroupsRecursive(MenuDefinitionBase menu, StandardMenuItem menuModel)
        {
            var groups = _menuItemGroups
                .Where(x => x.Parent == menu)
                .OrderBy(x => x.SortOrder)
                .ToList();

            for (int i = 0; i < groups.Count; ++i)
            {
                MenuItemGroupDefinition group = groups[i];
                var menuItems = _menuItems
                    .Where(x => x.Group == group)
                    .OrderBy(x => x.SortOrder);

                foreach (MenuItemDefinition menuItem in menuItems)
                {
                    StandardMenuItem menuItemModel = (menuItem.CommandDefinition != null)
                        ? new CommandMenuItem(_commandService.GetCommand(menuItem.CommandDefinition), menuModel)
                        : new TextMenuItem(menuItem) as StandardMenuItem;
                    AddGroupsRecursive(menuItem, menuItemModel);
                    menuModel.Add(menuItemModel);
                }

                if (i < groups.Count - 1 && menuItems.Any())
                    menuModel.Add(new MenuItemSeparator());
            }
        }
    }
}
