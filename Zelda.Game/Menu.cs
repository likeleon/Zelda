using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public interface IMenuContext { }

    public abstract class Menu : IInputEventHandler, ITimerContext, IMenuContext
    {
        public event EventHandler Started;
        public event EventHandler Finished;

        class MenuData
        {
            public Menu Menu { get; set; }
            public IMenuContext Context { get; set; }
            public bool RecentlyAdded { get; set; }

            public MenuData(Menu menu, IMenuContext context)
            {
                Menu = menu;
                Context = context;
                RecentlyAdded = true;
            }
        }

        static List<MenuData> _menus = new List<MenuData>();

        public static void Start(IMenuContext context, Menu menu, bool onTop = true)
        {
            if (onTop)
                _menus.Add(new MenuData(menu, context));
            else
                _menus.Insert(0, new MenuData(menu, context));

            menu.OnStarted();
            menu.Started?.Invoke(menu, EventArgs.Empty);
        }

        internal static void UpdateMenus()
        {
            for (int i = _menus.Count - 1; i >= 0; --i)
            {
                var menu = _menus[i];

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

        internal static void MenusOnDraw(IMenuContext context, Surface dstSurface)
        {
            foreach (var menu in _menus.Where(m => m.Context == context))
                MenuOnDraw(menu.Menu, dstSurface);
        }

        static void MenuOnDraw(Menu menu, Surface dstSurface)
        {
            menu.OnDraw(dstSurface);
            MenusOnDraw(menu, dstSurface);  // 자식 메뉴들을 그려줍니다
        }

        public void Stop()
        {
            var menuData = _menus.Find(m => m.Menu == this);
            if (menuData == null)
                return;

            menuData.Menu = null;
            menuData.Context = null;
            MenuOnFinished(this);
        }

        public static void StopAll(IMenuContext context)
        {
            RemoveMenus(context);
        }

        static void MenuOnFinished(Menu menu)
        {
            RemoveMenus(menu);  // 먼저 모든 자식 메뉴들을 정지시킵니다
            menu.OnFinished();
            menu.Finished?.Invoke(menu, EventArgs.Empty);
            Timer.StopAll(menu); // 이 메뉴에 관련된 타이머들을 모두 정지시킵니다
        }

        // context와 관련된 모든 메뉴를 해제합니다
        internal static void RemoveMenus(IMenuContext context)
        {
            // 어떤 메뉴들은 OnFinished 시점에 새로운 메뉴들을 생성할 수 있는데, 이들은 제거되지 않아야 합니다
            _menus.ForEach(m => m.RecentlyAdded = false);

            foreach (var menuData in _menus.Where(m => m.Context == context && !m.RecentlyAdded))
            {
                var menu = menuData.Menu;
                menuData.Menu = null;
                menuData.Context = null;
                MenuOnFinished(menu);
            }
        }

        internal static bool MenusOnInput(IMenuContext context, InputEvent input)
        {
            foreach (var menu in _menus.Where(m => m.Context == context).Reverse())
            {
                if (MenuOnInput(menu.Menu, input))
                    return true;
            }
            return false;
        }

        static bool MenuOnInput(Menu menu, InputEvent input)
        {
            // 자식 메뉴들에게 먼저 이벤트를 보냅니다
            bool handled = MenusOnInput(menu, input);

            if (!handled)
                handled = menu.OnInput(input);

            return handled;
        }

        public bool IsStarted() => _menus.Any(m => m.Menu == this);

        public virtual bool OnKeyPressed(KeyboardKey key, Modifiers modifiers) => false;
        public virtual bool OnKeyReleased(KeyboardKey key) => false;
        public virtual bool OnCharacterPressed(string character) => false;

        internal static bool OnCommandPressed(IMenuContext context, GameCommand command)
        {
            bool handled = false;
            foreach (var menu in _menus.Where(m => m.Context == context).Reverse())
                handled = MenuOnCommandPressed(menu.Menu, command);
            return handled;
        }

        static bool MenuOnCommandPressed(Menu menu, GameCommand command)
        {
            // 자식들이 먼저 처리하게 합니다
            if (OnCommandPressed(menu, command))
                return true;

            return menu.OnCommandPressed(command);
        }

        protected virtual void OnStarted() { }
        protected virtual void OnDraw(Surface dstSurface) { }
        protected virtual void OnFinished() { }
        protected virtual bool OnCommandPressed(GameCommand command) => false;
    }
}
