using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    interface IMod
    {
        event EventHandler Loaded;
        event EventHandler Unloaded;

        bool IsLoaded { get; }
        string RootPath { get; }
        string Name { get; }
        IModFile RootDirectory { get; }

        void Load(string modPath);
        void Unload();

        bool IsDirectory(string path);
        bool IsModRootDirectory(string path);
        bool IsResourceDirectory(string path, ref ResourceType resourceType);
        bool IsInResourceDirectory(string path, ref ResourceType resourceType);
        bool IsResourceElement(string path, ref ResourceType resourceType, ref string elementId);
        bool IsPotentialResourceElement(string path, ref ResourceType resourceType, ref string elementId);
    }
}
