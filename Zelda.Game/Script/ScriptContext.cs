using System;
using System.Linq;
using System.Reflection;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        static ObjectCreator _objectCreator;
        static ScriptMain _scriptMain;

        #region 메인 루프
        public static void Initialize()
        {
            _objectCreator = new ObjectCreator(MainLoop.CurrentMod.Resources);

            CreateScriptMain();
            CoreToScript.Call(_scriptMain.OnStarted);
        }

        public static void Exit()
        {
            if (_scriptMain != null)
                CoreToScript.Call(_scriptMain.OnFinished);
            
            ScriptMain.Current = null;

            ScriptMenu.DestroyMenus();
            ScriptTimer.DestroyTimers();
            ScriptDrawable.DestroyDrawables();
        }

        public static void Update()
        {
            ScriptDrawable.UpdateDrawables();
            ScriptMenu.UpdateMenus();
            ScriptTimer.UpdateTimers();

            CoreToScript.Call(_scriptMain.OnUpdate);
        }

        static void CreateScriptMain()
        {
            var mainTypes = _objectCreator.GetTypesImplementing<ScriptMain>();
            if (!mainTypes.Any())
                Debug.Error("'Main' based class not found");

            if (mainTypes.Count() > 1)
                Debug.Error("'Too many 'Main' based classes");

            Type mainType = mainTypes.First();
            ConstructorInfo ctor = mainType.GetConstructor(Type.EmptyTypes);
            _scriptMain = (ScriptMain)ctor.Invoke(null);
            ScriptMain.Current = _scriptMain;
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
            CoreToScript.Call(() => _scriptMain.OnDraw(dstSurface));
            ScriptMenu.MenusOnDraw(_scriptMain, dstSurface);
        }

        internal static void GameOnDraw(Game game, ScriptSurface dstSurface)
        {
            game.SaveGame.ScriptGame.NotifyDraw(dstSurface);
            ScriptMenu.MenusOnDraw(game.SaveGame.ScriptGame, dstSurface);
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
            else if (input.IsCharacterPressed)
                handled = OnCharacterPressed(handler, input) || handled;

            return handled;
        }

        static bool OnKeyPressed(IInputEventHandler handler, InputEvent input)
        {
            return CoreToScript.Call(() =>
            {
                return handler.OnKeyPressed(input.KeyboardKey, new Modifiers(input));
            });
        }

        static bool OnKeyReleased(IInputEventHandler handler, InputEvent input)
        {
            return CoreToScript.Call(() =>
            {
                return handler.OnKeyReleased(input.KeyboardKey);
            });
        }

        static bool OnCharacterPressed(IInputEventHandler handler, InputEvent input)
        {
            return CoreToScript.Call(() =>
            {
                return handler.OnCharacterPressed(input.Character);
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
