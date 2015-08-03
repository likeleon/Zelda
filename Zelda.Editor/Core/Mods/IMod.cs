using System;

namespace Zelda.Editor.Core.Mods
{
    interface IMod
    {
        event EventHandler Loaded;
        event EventHandler Unloaded;

        bool IsLoaded { get; }
        string RootPath { get; }
        string Name { get; }

        void Load(string modPath);
        void Unload();
    }
}
