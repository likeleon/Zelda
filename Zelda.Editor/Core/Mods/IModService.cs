using System;

namespace Zelda.Editor.Core.Mods
{
    interface IModService
    {
        event EventHandler<IMod> Loaded;
        event EventHandler<IMod> Unloaded;

        bool IsLoaded { get; }
        IMod Mod { get; }

        void Load(string rootPath);
        void Unload();
    }
}
