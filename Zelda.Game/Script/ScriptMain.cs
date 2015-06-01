using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public abstract class ScriptMain : IInputEventHandler
    {
        public static ScriptMain Current { get; internal set; }

        public bool LoadSettings(string fileName = null)
        {
            return ScriptToCore.Call(() =>
            {
                fileName = fileName ?? "settings.dat";

                if (String.IsNullOrEmpty(ModFiles.ModWriteDir))
                    throw new Exception("Cannnot load settings: no write directory was specified in mod.xml");

                return Settings.Load(fileName);
            });
        }

        public bool SaveSettings(string fileName = null)
        {
            return ScriptToCore.Call(() =>
            {
                fileName = fileName ?? "settings.dat";

                if (String.IsNullOrEmpty(ModFiles.ModWriteDir))
                    throw new Exception("Cannnot save settings: no write directory was specified in mod.xml");

                return Settings.Save(fileName);
            });
        }

        public void Exit()
        {
            ScriptToCore.Call(() => ScriptContext.MainLoop.Exiting = true);
        }

        public void Reset()
        {
            ScriptToCore.Call(ScriptContext.MainLoop.SetResetting);
        }

        public virtual bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            return false;
        }

        public virtual bool OnKeyReleased(KeyboardKey key)
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
