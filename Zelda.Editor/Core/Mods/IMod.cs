using System.Collections.Generic;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    interface IMod
    {
        string RootPath { get; }
        string ResourceListPath { get; }
        string Name { get; }
        IModFile RootDirectory { get; }
        ModResources Resources { get; }

        bool Exists(string path);

        string GetResourceElementPath(ResourceType resourceType, string elementId);
        IEnumerable<string> GetResourceElementPaths(ResourceType resourceType, string elementId);
        string GetFontPath(string fontId);
        string GetSoundPath(string soundId);
        string GetLanguagePath(string languageId);
        string GetMapDataFilePath(string mapId);
        string GetTilesetDataFilePath(string tilesetId);
        string GetTilesetTilesImagePath(string tilesetId);
        string GetTilesetEntitiesImagePath(string tilesetId);
        string GetSpritePath(string spriteId);
        string GetMusicPath(string musicId);

        bool IsDirectory(string path);
        bool IsModRootDirectory(string path);
        bool IsResourceDirectory(string path, ref ResourceType resourceType);
        bool IsInResourceDirectory(string path, ref ResourceType resourceType);
        bool IsResourceElement(string path, ref ResourceType resourceType, ref string elementId);
        bool IsPotentialResourceElement(string path, ref ResourceType resourceType, ref string elementId);
    }
}
