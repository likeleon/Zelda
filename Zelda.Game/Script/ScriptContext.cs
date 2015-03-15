using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Zelda.Game.Engine;

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

        public static void Initialize(MainLoop mainLoop)
        {
            _mainLoop = mainLoop;
            _objectCreator = new ObjectCreator(CurrentMod.Resources);

            CreateScriptMain();
            _scriptMain.OnStarted();
        }

        public static void Exit()
        {
            if (_scriptMain != null)
                _scriptMain.OnFinished();

            DestroyMenus();
            DestroyTimers();
            DestroyDrawables();
        }

        public static void Update()
        {
            UpdateDrawables();
            UpdateMenus();
            UpdateTimers();

            _scriptMain.OnUpdate();
        }

        static void CreateScriptMain()
        {
            var mainTypes = _objectCreator.GetTypesImplementing<Main>();
            if (mainTypes.Count() <= 0)
                throw new InvalidDataException("'Main' based class not found");

            if (mainTypes.Count() > 1)
                throw new InvalidDataException("'Too many 'Main' based classes");

            Type mainType = mainTypes.First();
            ConstructorInfo ctor = mainType.GetConstructor(new Type[] { typeof(MainLoop) });
            _scriptMain = (Main)ctor.Invoke(new object[] { _mainLoop });
        }

        static bool OnInput(IInputEventHandler handler, InputEvent input)
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
            string keyName = InputEvent.GetKeyboardKeyName(input.KeyboardKey);
            bool shift = input.IsWithShift;
            bool control = input.IsWithControl;
            bool alt = input.IsWithAlt;
            return handler.OnKeyPressed(keyName, shift, control, alt);
        }

        static bool OnKeyReleased(IInputEventHandler context, InputEvent input)
        {
            string keyName = InputEvent.GetKeyboardKeyName(input.KeyboardKey);
            return context.OnKeyReleased(keyName);
        }
    }
}
