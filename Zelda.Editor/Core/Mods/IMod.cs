using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    interface IMod
    {
        string RootPath { get; }
        string ResourceListPath { get; }
        string Name { get; }
        IModFile RootDirectory { get; }

        bool IsDirectory(string path);
        bool IsModRootDirectory(string path);
        bool IsResourceDirectory(string path, ref ResourceType resourceType);
        bool IsInResourceDirectory(string path, ref ResourceType resourceType);
        bool IsResourceElement(string path, ref ResourceType resourceType, ref string elementId);
        bool IsPotentialResourceElement(string path, ref ResourceType resourceType, ref string elementId);
    }
}
