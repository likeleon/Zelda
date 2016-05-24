using System;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public abstract class Main : IInputEventHandler, ITimerContext, IMenuContext, IDisposable
    {
        public static Main Current { get; internal set; }

        public bool LoadSettings(string fileName = null)
        {
            fileName = fileName ?? "settings.dat";

            if (Core.Mod.ModFiles.ModWriteDir == null)
                throw new Exception("Cannnot load settings: no write directory was specified in mod.xml");

            return Settings.Load(fileName);
        }

        public bool SaveSettings(string fileName = null)
        {
            fileName = fileName ?? "settings.dat";

            if (Core.Mod.ModFiles.ModWriteDir == null)
                throw new Exception("Cannnot save settings: no write directory was specified in mod.xml");

            return Settings.Save(fileName);
        }

        public void Exit() => Core.Exiting = true;

        public virtual bool OnKeyPressed(KeyboardKey key, Modifiers modifiers) => false;

        public virtual bool OnKeyReleased(KeyboardKey key) => false;
        public virtual bool OnCharacterPressed(string character) => false;

        internal protected virtual void OnStarted() { }
        internal protected virtual void OnFinished() { }
        internal protected virtual void OnUpdate() { }
        internal protected virtual void OnDraw(Surface dstSurface) { }

        public abstract void Dispose();
    }
}
