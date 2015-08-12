using System;

namespace Zelda.Editor.Core.Mods
{
    interface IModService
    {
        event EventHandler Loaded;
        event EventHandler Unloaded;

        bool IsLoaded { get; }
        IMod Mod { get; }

        void Load(string rootPath);
        void Unload();
    }
}
