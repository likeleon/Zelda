using System;
using System.Collections.Generic;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        class ScriptMenuData
        {
            readonly Menu _menu;
            public Menu Menu
            {
                get { return _menu; }
            }

            readonly object _context;
            public object Context
            {
                get { return _context; }
            }

            public bool RecentlyAdded { get; set; }

            public ScriptMenuData(Menu menu, object context)
            {
                _menu = menu;
                _context = context;
                RecentlyAdded = true;
            }
        }

        static List<ScriptMenuData> _menus = new List<ScriptMenuData>();

        public static void AddMenu(Menu menu, object context, bool onTop)
        {
            if (onTop)
                _menus.Add(new ScriptMenuData(menu, context));
            else
                _menus.Insert(0, new ScriptMenuData(menu, context));

            menu.OnStarted();
        }

        static void UpdateMenus()
        {
            for (int i = _menus.Count - 1; i >= 0; --i)
            {
                ScriptMenuData menu = _menus[i];
                
                menu.RecentlyAdded = false;
                if (menu.Menu ==  null)
                {
                    if (menu.Context != null)
                        throw new InvalidOperationException("Menu with context and no ref");
                    
                    _menus.RemoveAt(i);
                }
            }
        }

        static void DestroyMenus()
        {
            _menus.Clear();
        }
    }
}
