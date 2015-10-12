using System;
using Zelda.Editor.Modules.Mods.Models;

namespace Zelda.Editor.Modules.Mods.Services
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
