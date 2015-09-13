using System;
using System.Collections.Generic;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    interface IMod
    {
        event EventHandler<string> FileCreated;
        event EventHandler<string> FileDeleted;
        event EventHandler<FileRenamedEventArgs> FileRenamed;

        string Name { get; }
        string RootPath { get; }
        string MainScriptPath { get; }
        ModResources Resources { get; }

        bool Exists(string path);
        void CheckExists(string path);
        void CheckNotExists(string path);

        string GetResourceListPath();
        string GetResourcePath(ResourceType resourceType);
        string GetResourceElementPath(ResourceType resourceType, string elementId);
        IEnumerable<string> GetResourceElementPaths(ResourceType resourceType, string elementId);
        string GetFontPath(string fontId);
        string GetSoundPath(string soundId);
        string GetLanguagePath(string languageId);
        string GetMapDataFilePath(string mapId);
        string GetMapScriptPath(string mapId);
        string GetTilesetDataFilePath(string tilesetId);
        string GetTilesetTilesImagePath(string tilesetId);
        string GetTilesetEntitiesImagePath(string tilesetId);
        string GetSpritePath(string spriteId);
        string GetMusicPath(string musicId);
        string GetDialogsPath(string languageId);
        string GetStringsPath(string languageId);

        bool IsValidFileName(string fileName);
        void CheckValidFileName(string fileName);
        bool IsInRootPath(string path);
        void CheckIsInRootPath(string path);
        bool IsModRootDirectory(string path);
        bool IsDialogsFile(string path, ref string languageId);
        bool IsStringsFile(string path, ref string languageId);
        bool IsDirectory(string path);
        bool IsResourceDirectory(string path, ref ResourceType resourceType);
        bool IsInResourceDirectory(string path, ref ResourceType resourceType);
        bool IsScript(string path);
        bool IsResourceElement(string path, ref ResourceType resourceType, ref string elementId);
        bool IsPotentialResourceElement(string path, ref ResourceType resourceType, ref string elementId);

        void CreateResourceElement(ResourceType resourceType, string elementId, string description);
        void CreateDirectory(string path);
        bool CreateDirectoryIfNotExists(string path);
        void CreateDirectory(string parentPath, string dirName);

        void RenameFile(string oldPath, string newPath);
        bool RenameFileIfExists(string oldPath, string newPath);
        void RenameResourceElement(ResourceType resourceType, string oldId, string newId);

        void DeleteFile(string path);
        bool DeleteFileIfExists(string path);
        void DeleteDirectory(string path);
        bool DeleteDirectoryIfExists(string path);
        void DeleteDirectoryRecursive(string path);
        void DeleteResourceElement(ResourceType resourceType, string elementId);
    }
}
