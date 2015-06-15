using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public abstract class ScriptMenu : IInputEventHandler
    {
        internal class ScriptMenuData
        {
            public ScriptMenu Menu { get; set; }
            public object Context { get; set; }
            public bool RecentlyAdded { get; set; }

            public ScriptMenuData(ScriptMenu menu, object context)
            {
                Menu = menu;
                Context = context;
                RecentlyAdded = true;
            }
        }

        static List<ScriptMenuData> _menus = new List<ScriptMenuData>();

        public event EventHandler Started = delegate { };
        public event EventHandler Finished = delegate { };

        public static void Start(object context, ScriptMenu menu, bool onTop = true)
        {
            ScriptToCore.Call(() => AddMenu(menu, context, onTop));
        }

        static void AddMenu(ScriptMenu menu, object context, bool onTop)
        {
            if (onTop)
                _menus.Add(new ScriptMenuData(menu, context));
            else
                _menus.Insert(0, new ScriptMenuData(menu, context));

            menu.NotifyStarted();
        }

        internal static void UpdateMenus()
        {
            for (int i = _menus.Count - 1; i >= 0; --i)
            {
                ScriptMenuData menu = _menus[i];

                menu.RecentlyAdded = false;
                if (menu.Menu == null)
                {
                    if (menu.Context != null)
                        throw new InvalidOperationException("Menu with context and no ref");

                    _menus.RemoveAt(i);
                }
            }
        }

        internal static void DestroyMenus()
        {
            _menus.Clear();
        }

        internal static void MenusOnDraw(object context, ScriptSurface dstSurface)
        {
            foreach (var menu in _menus.Where(m => m.Context == context))
                MenuOnDraw(menu.Menu, dstSurface);
        }

        static void MenuOnDraw(ScriptMenu menu, ScriptSurface dstSurface)
        {
            CoreToScript.Call(() => menu.OnDraw(dstSurface));
            MenusOnDraw(menu, dstSurface);  // 자식 메뉴들을 그려줍니다
        }

        public static bool IsStarted(ScriptMenu menu)
        {
            return _menus.Any(m => m.Menu == menu);
        }

        public static void Stop(ScriptMenu menu)
        {
            ScriptMenuData menuData = _menus.Find(m => m.Menu == menu);
            if (menuData != null)
            {
                menuData.Menu = null;
                menuData.Context = null;
                MenuOnFinished(menu);
            }
        }

        static void MenuOnFinished(ScriptMenu menu)
        {
            RemoveMenus(menu);  // 먼저 모든 자식 메뉴들을 정지시킵니다
            menu.NotifyFinished();
            ScriptTimer.RemoveTimers(menu); // 이 메뉴에 관련된 타이머들을 모두 정지시킵니다
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

        internal static bool MenusOnInput(object context, InputEvent input)
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

        static bool MenuOnInput(ScriptMenu menu, InputEvent input)
        {
            // 자식 메뉴들에게 먼저 이벤트를 보냅니다
            bool handled = MenusOnInput(menu, input);

            if (!handled)
                handled = ScriptContext.OnInput(menu, input);

            return handled;
        }

        protected virtual void OnStarted()
        {
        }
        
        protected internal virtual void OnDraw(ScriptSurface dstSurface)
        {
        }

        protected virtual void OnFinished()
        {
        }

        public bool IsStarted()
        {
            return ScriptToCore.Call(() => IsStarted(this));
        }

        public void Stop()
        {
            ScriptToCore.Call(() => Stop(this));
        }

        internal void NotifyStarted()
        {
            CoreToScript.Call(() =>
            {
                OnStarted();
                Started(this, EventArgs.Empty);
            });
        }

        internal void NotifyFinished()
        {
            CoreToScript.Call(() =>
            {
                OnFinished();
                Finished(this, EventArgs.Empty);
            });
        }

        public virtual bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            return false;
        }

        public virtual bool OnKeyReleased(KeyboardKey key)
        {
            return false;
        }
    }
}
