﻿using System;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Script
{
    public abstract class ScriptMain : IInputEventHandler, ITimerContext, IMenuContext
    {
        public static ScriptMain Current { get; internal set; }

        public bool LoadSettings(string fileName = null)
        {
            return ScriptToCore.Call(() =>
            {
                fileName = fileName ?? "settings.dat";

                if (String.IsNullOrEmpty(Core.Mod.ModFiles.ModWriteDir))
                    throw new Exception("Cannnot load settings: no write directory was specified in mod.xml");

                return Settings.Load(fileName);
            });
        }

        public bool SaveSettings(string fileName = null)
        {
            return ScriptToCore.Call(() =>
            {
                fileName = fileName ?? "settings.dat";

                if (String.IsNullOrEmpty(Core.Mod.ModFiles.ModWriteDir))
                    throw new Exception("Cannnot save settings: no write directory was specified in mod.xml");

                return Settings.Save(fileName);
            });
        }

        public static void Exit()
        {
            ScriptToCore.Call(() => Core.Exiting = true);
        }

        public static void Reset()
        {
            ScriptToCore.Call(Core.SetResetting);
        }

        public virtual bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            return false;
        }

        public virtual bool OnKeyReleased(KeyboardKey key)
        {
            return false;
        }

        public virtual bool OnCharacterPressed(string character)
        {
            return false;
        }

        internal protected virtual void OnStarted()
        {
        }

        internal protected virtual void OnFinished()
        {
        }

        internal protected virtual void OnUpdate()
        {
        }

        internal protected virtual void OnDraw(ScriptSurface dstSurface)
        {
        }
    }
}
