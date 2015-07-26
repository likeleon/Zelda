using System;

namespace Zelda.Editor.Modules.ModEditor.Services
{
    interface IModService
    {
        event EventHandler ModLoaded;
        event EventHandler ModUnloaded;

        bool IsLoaded { get; }
        string RootPath { get; }

        void LoadMod(string modPath);
        void UnloadMod();
    }
}
