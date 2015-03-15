using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        internal class ScriptMenuData
        {
            public Menu Menu { get; set; }
            public object Context { get; set; }
            public bool RecentlyAdded { get; set; }

            public ScriptMenuData(Menu menu, object context)
            {
                Menu = menu;
                Context = context;
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

            menu.NotifyStarted();
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

        static void MenusOnDraw(object context, Surface dstSurface)
        {
            ScriptMenuData menu = _menus.Find(m => m.Context == context);
            if (menu != null)
                MenuOnDraw(menu.Menu, dstSurface);
        }

        static void MenuOnDraw(Menu menu, Surface dstSurface)
        {
            menu.OnDraw(dstSurface);
        }

        public static bool IsStarted(Menu menu)
        {
            return _menus.Any(m => m.Menu == menu);
        }

        public static void Stop(Menu menu)
        {
            ScriptMenuData menuData = _menus.Find(m => m.Menu == menu);
            if (menuData != null)
            {
                menuData.Menu = null;
                menuData.Context = null;
                MenuOnFinished(menu);
            }
        }

        static void MenuOnFinished(Menu menu)
        {
            RemoveMenus(menu);  // 먼저 모든 자식 메뉴들을 정지시킵니다
            menu.NotifyFinished();
            RemoveTimers(menu); // 이 메뉴에 관련된 타이머들을 모두 정지시킵니다
        }

        // context와 관련된 모든 메뉴를 해제합니다
        static void RemoveMenus(object context)
        {
            // 어떤 메뉴들은 OnFinished 시점에 새로운 메뉴들을 생성할 수 있는데, 이들은 제거되지 않아야 합니다
            _menus.ForEach(m => m.RecentlyAdded = false);

            foreach (ScriptMenuData menu in _menus)
            {
                if (menu.Context == context && !menu.RecentlyAdded)
                {
                    menu.Menu = null;
                    menu.Context = null;
                    MenuOnFinished(menu.Menu);
                }
            }
        }

        static bool MenusOnInput(object context, InputEvent input)
        {
            bool handled = false;
            foreach (ScriptMenuData menu in Enumerable.Reverse(_menus))
            {
                if (menu.Context == context)
                    handled = MenuOnInput(menu.Menu, input);
                if (handled)
                    break;
            }
            return handled;
        }

        static bool MenuOnInput(Menu menu, InputEvent input)
        {
            // 자식 메뉴들에게 먼저 이벤트를 보냅니다
            bool handled = MenusOnInput(menu, input);

            if (!handled)
                handled = OnInput(menu, input);

            return handled;
        }
    }
}
