using System;

namespace Zelda.Editor.Modules.ModEditor.Services
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
