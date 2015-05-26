using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        static MainLoop _mainLoop;
        internal static MainLoop MainLoop
        {
            get { return _mainLoop; }
        }

        static ObjectCreator _objectCreator;
        static Main _scriptMain;

        #region 메인 루프
        public static void Initialize(MainLoop mainLoop)
        {
            _mainLoop = mainLoop;
            _objectCreator = new ObjectCreator(CurrentMod.Resources);

            CreateScriptMain();
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _scriptMain.OnStarted();
            });
        }

        public static void Exit()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (_scriptMain != null)
                    _scriptMain.OnFinished();
            });
            Main.Current = null;

            ScriptMenu.DestroyMenus();
            ScriptTimer.DestroyTimers();
            ScriptDrawable.DestroyDrawables();
        }

        public static void Update()
        {
            ScriptDrawable.UpdateDrawables();
            ScriptMenu.UpdateMenus();
            ScriptTimer.UpdateTimers();

            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _scriptMain.OnUpdate();
            });
        }

        static void CreateScriptMain()
        {
            var mainTypes = _objectCreator.GetTypesImplementing<Main>();
            if (!mainTypes.Any())
                Debug.Error("'Main' based class not found");

            if (mainTypes.Count() > 1)
                Debug.Error("'Too many 'Main' based classes");

            Type mainType = mainTypes.First();
            ConstructorInfo ctor = mainType.GetConstructor(Type.EmptyTypes);
            _scriptMain = (Main)ctor.Invoke(null);
            Main.Current = _scriptMain;
        }

        internal static void NotifyDialogFinished(Zelda.Game.Game game, Dialog dialog, Action callback)
        {
            if (callback != null)
                callback();
        }

        internal static ScriptItem RunItem(EquipmentItem item)
        {
            string className =  GetScriptClassName<ScriptItem>(item.Name);
            if (className == null)
                return null;

            ScriptItem scriptItem = _objectCreator.CreateObject<ScriptItem>(className);
            scriptItem.NotifyCreated(item);
            return scriptItem;
        }

        static string GetScriptClassName<T>(string id)
        {
            var itemTypes = _objectCreator.GetTypesImplementing<T>();
            if (!itemTypes.Any())
                return null;

            foreach (Type itemType in itemTypes)
            {
                string idValue = itemType.GetCustomAttributes<IdAttribute>().DefaultIfEmpty(IdAttribute.Default).First().Id;
                if (idValue == id)
                    return itemType.Name;
            }

            return null;
        }

        internal static ScriptMap CreateScriptMap(Map map)
        {
            string className = GetScriptClassName<ScriptMap>(map.Id);
            if (className == null)
                Debug.Die("Cannot find script file for map '{0}'".F(map.Id));

            ScriptMap scriptMap = _objectCreator.CreateObject<ScriptMap>(className);
            scriptMap.Map = map;
            return scriptMap;
        }

        internal static void MainOnDraw(ScriptSurface dstSurface)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _scriptMain.OnDraw(dstSurface);
            });
            ScriptMenu.MenusOnDraw(_scriptMain, dstSurface);
        }

        internal static bool MainOnInput(InputEvent inputEvent)
        {
            bool handled = OnInput(_scriptMain, inputEvent);
            if (!handled)
                handled = ScriptMenu.MenusOnInput(_scriptMain, inputEvent);
            return handled;
        }
        #endregion

        #region 이벤트들
        internal static bool OnInput(IInputEventHandler handler, InputEvent input)
        {
            bool handled = false;
            if (input.IsKeyboardEvent)
            {
                if (input.IsKeyboardKeyPressed)
                    handled = OnKeyPressed(handler, input) || handled;
                else if (input.IsKeyboardKeyReleased)
                    handled = OnKeyReleased(handler, input) || handled;
            }
            return handled;
        }

        static bool OnKeyPressed(IInputEventHandler handler, InputEvent input)
        {
            return ScriptTools.ExceptionBoundaryHandle<bool>(() =>
            {
                string keyName = InputEvent.GetKeyboardKeyName(input.KeyboardKey);
                bool shift = input.IsWithShift;
                bool control = input.IsWithControl;
                bool alt = input.IsWithAlt;
                return handler.OnKeyPressed(keyName, shift, control, alt);
            });
        }

        static bool OnKeyReleased(IInputEventHandler context, InputEvent input)
        {
            return ScriptTools.ExceptionBoundaryHandle<bool>(() =>
            {
                string keyName = InputEvent.GetKeyboardKeyName(input.KeyboardKey);
                return context.OnKeyReleased(keyName);
            });
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class IdAttribute : Attribute
    {
        public static readonly IdAttribute Default = new IdAttribute(String.Empty);

        public string Id;

        public IdAttribute(string id)
        {
            Id = id;
        }
    }
}
