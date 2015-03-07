﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

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

        internal static void MainOnDraw(Surface dstSurface)
        {
            _scriptMain.OnDraw(dstSurface);
            MenusOnDraw(_scriptMain, dstSurface);
        }
    }
}
