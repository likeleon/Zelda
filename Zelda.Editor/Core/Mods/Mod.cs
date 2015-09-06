using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class Mod : PropertyChangedBase, IMod
    {
        public event EventHandler<string> FileCreated;

        static readonly Dictionary<ResourceType, string> _resourceDirs = new Dictionary<ResourceType, string>()
        {
            { ResourceType.Map,      "maps"      },
            { ResourceType.Tileset,  "tilesets"  },
            { ResourceType.Sprite,   "sprites"   },
            { ResourceType.Music,    "musics"    },
            { ResourceType.Sound,    "sounds"    },
            { ResourceType.Item,     "items"     },
            { ResourceType.Enemy,    "enemies"   },
            { ResourceType.Entity,   "entities"  },
            { ResourceType.Language, "languages" },
            { ResourceType.Font,     "fonts"     },
        };

        public ModResources Resources { get; private set; }
        public ModProperties Properties { get; private set; }

        public Mod(ModProperties properties)
        {
            RootPath = properties.ModPath.RootPath;
            Name = Path.GetFileName(RootPath);
            Resources = ModResources.Load(GetResourceListPath(), this);
            Properties = properties;
        }

        #region Get paths
        public string RootPath { get; private set; }
        // TODO: 어셈블리를 열어서 "ScriptMain"을 상속한 클래스가 정의된 파일에 대한 경로를 반환해야 한다
        public string MainScriptPath {  get { return Path.Combine(RootPath, "scripts", "Main.cs"); } }

        public string Name { get; private set; }

        public string GetResourceListPath()
        {
            return Path.Combine(RootPath, "project_db.xml");
        }

        public string GetResourceElementPath(ResourceType resourceType, string elementId)
        {
            return GetResourceElementPaths(resourceType, elementId).FirstOrDefault();
        }

        public IEnumerable<string> GetResourceElementPaths(ResourceType resourceType, string elementId)
        {
            switch (resourceType)
            {
                case ResourceType.Language:
                    yield return GetLanguagePath(elementId);
                    break;

                case ResourceType.Map:
                    yield return GetMapDataFilePath(elementId);
                    //yield return GetMapScriptPath(elementId);
                    break;

                case ResourceType.Tileset:
                    yield return GetTilesetDataFilePath(elementId);
                    yield return GetTilesetTilesImagePath(elementId);
                    yield return GetTilesetEntitiesImagePath(elementId);
                    break;

                case ResourceType.Sprite:
                    yield return GetSpritePath(elementId);
                    break;

                case ResourceType.Music:
                    yield return GetMusicPath(elementId);
                    break;

                case ResourceType.Sound:
                    yield return GetSoundPath(elementId);
                    break;

                case ResourceType.Item:
                    //yield return GetItemScriptPath(elementId);
                    break;

                case ResourceType.Enemy:
                    //yield return GetEnemyScriptPath(elementId);
                    break;

                case ResourceType.Entity:
                    //yield return GetEntityScriptPath(elementId);
                    break;

                case ResourceType.Font:
                    yield return GetFontPath(elementId);
                    break;

                default:
                    yield break;
            }
        }

        public string GetFontPath(string fontId)
        {
            var prefix = Path.Combine(RootPath, "fonts", fontId);
            var extensions = new[] { ".png", ".ttf", ".ttc", ".fon" };
            foreach (var extension in extensions)
            {
                var path = prefix + extension;
                if (File.Exists(path))
                    return path;
            }
            return prefix + extensions.First();
        }

        public string GetSoundPath(string soundId)
        {
            return Path.Combine(RootPath, "sounds", "{0}.ogg".F(soundId));
        }

        public string GetLanguagePath(string languageId)
        {
            return Path.Combine(RootPath, "languages", languageId);
        }

        public string GetLanguageImagesPath(string languageId)
        {
            return Path.Combine(GetLanguagePath(languageId), "images");
        }

        public string GetLanguageTextPath(string languageId)
        {
            return Path.Combine(GetLanguagePath(languageId), "text");
        }

        public string GetMapDataFilePath(string mapId)
        {
            return Path.Combine(RootPath, "maps", "{0}.xml".F(mapId));
        }

        public string GetTilesetDataFilePath(string tilesetId)
        {
            return Path.Combine(RootPath, "tilesets", "{0}.xml".F(tilesetId));
        }

        public string GetTilesetTilesImagePath(string tilesetId)
        {
            return Path.Combine(RootPath, "tilesets", "{0}.tiles.pngs".F(tilesetId));
        }

        public string GetTilesetEntitiesImagePath(string tilesetId)
        {
            return Path.Combine(RootPath, "tilesets", "{0}.entities.png".F(tilesetId));
        }

        public string GetSpritePath(string spriteId)
        {
            return Path.Combine(RootPath, "sprites", "{0}.xml".F(spriteId));
        }

        public string GetDialogsPath(string languageId)
        {
            return Path.Combine(GetLanguagePath(languageId), "text", "dialogs.xml");
        }

        public string GetStringsPath(string languageId)
        {
            return Path.Combine(GetLanguagePath(languageId), "text", "strings.xml");
        }

        public string GetMusicPath(string musicId)
        {
            var prefix = Path.Combine(RootPath, "musics", musicId);
            var extensions = new[] { ".ogg", ".it", ".spc" };
            foreach (var extension in extensions)
            {
                var path = prefix + extension;
                if (File.Exists(path))
                    return path;
            }
            return prefix + extensions.First();
        }

        public string GetResourcePath(ResourceType resourceType)
        {
            string dirName;
            if (!_resourceDirs.TryGetValue(resourceType, out dirName))
                return "";

            return Path.Combine(RootPath, dirName);
        }
        #endregion

        #region Check path properties
        static bool ModExists(string rootPath)
        {
            var modPath = new ModPath(rootPath);
            return modPath.Exists(modPath.PropertiesPath);
        }

        public bool IsDirectory(string path)
        {
            return Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public void CheckIsDirectory(string path)
        {
            CheckExists(path);

            if (!Directory.Exists(path))
                throw new Exception("File '{0}' is not a folder".F(path));
        }

        public void CheckNotIsDirectory(string path)
        {
            CheckExists(path);

            if (Directory.Exists(path))
                throw new Exception("File '{0}' is a folder".F(path));
        }

        public bool Exists(string path)
        {
            return IsInRootPath(path) && (File.Exists(path) || Directory.Exists(path));
        }

        public void CheckExists(string path)
        {
            if (!Exists(path))
                throw new Exception("File '{0}' does not exist".F(path));
        }

        public void CheckNotExists(string path)
        {
            CheckIsInRootPath(path);

            if (Exists(path))
                throw new Exception("File '{0}' already exists".F(path));
        }

        public bool IsInRootPath(string path)
        {
            return path.StartsWith(RootPath);
        }

        public void CheckIsInRootPath(string path)
        {
            if (!IsInRootPath(path))
                throw new Exception("File '{0}' is not in this mod".F(path));
        }

        public bool IsModRootDirectory(string path)
        {
            return path == RootPath;
        }

        public bool IsResourceDirectory(string path, ref ResourceType resourceType)
        {
            foreach (var type in _resourceDirs.Keys)
            {
                if (path == GetResourcePath(type))
                {
                    resourceType = type;
                    return true;
                }
            }
            return false;
        }

        public bool IsInResourceDirectory(string path, ref ResourceType resourceType)
        {
            foreach (var type in _resourceDirs.Keys)
            {
                if (path.StartsWith(GetResourcePath(type) + Path.DirectorySeparatorChar))
                {
                    resourceType = type;
                    return true;
                }
            }
            return false;
        }

        public bool IsScript(string path)
        {
            return IsInRootPath(path) && path.EndsWith(".cs");
        }

        public bool IsResourceElement(string path, ref ResourceType resourceType, ref string elementId)
        {
            if (!IsPotentialResourceElement(path, ref resourceType, ref elementId))
                return false;

            return Resources.Exists(resourceType, elementId);
        }

        public bool IsPotentialResourceElement(string path, ref ResourceType resourceType, ref string elementId)
        {
            elementId = string.Empty;

            if (!IsInResourceDirectory(path, ref resourceType))
                return false;

            if (IsResourceDirectory(path, ref resourceType))
                return false;

            return GetElementId(path, resourceType, ref elementId);
        }

        bool GetElementId(string path, ResourceType resourceType, ref string elementId)
        {
            var resourceDir = GetResourcePath(resourceType);
            var pathFromResource = path.Substring(resourceDir.Length + 1);
            var extensions = new List<string>();
            switch (resourceType)
            {
                case ResourceType.Map:
                case ResourceType.Tileset:
                case ResourceType.Sprite:
                    extensions.Add(".xml");
                    break;

                case ResourceType.Music:
                    extensions.AddRange(new[] { ".ogg", ".it", ".spc" });
                    break;

                case ResourceType.Sound:
                    extensions.Add(".ogg");
                    break;

                case ResourceType.Item:
                case ResourceType.Enemy:
                case ResourceType.Entity:
                    extensions.Add(".cs");
                    break;

                case ResourceType.Font:
                    extensions.AddRange(new[] { ".png", ".ttf", ".ttc", "fon" });
                    break;

                case ResourceType.Language:
                    // No extension
                    break;
            }

            if (resourceType == ResourceType.Language)
            {
                elementId = pathFromResource;
                return true;
            }
            else if (extensions.Any(x => pathFromResource.EndsWith(x)))
            {
                elementId = pathFromResource.Remove(pathFromResource.IndexOf('.'));
                return true;
            }
            else
                return false;
        }

        public bool IsValidFileName(string name)
        {
            if (name.IsNullOrEmpty() ||
                name.Contains('\\') ||
                name == "." ||
                name == ".." ||
                name.StartsWith("../") ||
                name.EndsWith("/..") ||
                name.Contains("/../") ||
                name.Trim() != name)
                return false;

            return true;
        }

        public void CheckValidFileName(string name)
        {
            if (!IsValidFileName(name))
            {
                if (name.IsNullOrEmpty())
                    throw new Exception("Empty file name");
                else
                    throw new Exception("Invalid file name: {0}".F(name));
            }
        }
        #endregion

        #region Create, Rename and Delete paths
        public void CreateFile(string path)
        {
            CheckIsInRootPath(path);
            CheckNotExists(path);

            var file = File.Create(path);
            file.Close();

            if (FileCreated != null)
                FileCreated(this, path);
        }

        public bool CreateFileIfNotExists(string path)
        {
            if (Exists(path))
            {
                CheckNotIsDirectory(path);
                return false;
            }

            CreateFile(path);
            return true;
        }

        public void CreateMapDataFile(string mapId)
        {
            var path = GetMapDataFilePath(mapId);
            CheckIsInRootPath(path);
            CheckNotExists(path);
            CreateFile(path);

            var map = new MapModel(this, mapId);
            var tilesetIds = Resources.GetElements(ResourceType.Tileset);
            if (tilesetIds.Any())
                map.SetTilesetId(tilesetIds.First());
            map.SetSize(Properties.NormalModSize);
            map.Save();
        }

        public bool CreateMapDataFileIfNotExists(string mapId)
        {
            var path = GetMapDataFilePath(mapId);
            if (Exists(path))
            {
                CheckNotIsDirectory(path);
                return false;
            }

            CreateMapDataFile(mapId);
            return true;
        }

        public void CreateDirectory(string path)
        {
            CheckIsInRootPath(path);
            CheckNotExists(path);

            Directory.CreateDirectory(path);
        }

        public bool CreateDirectoryIfNotExists(string path)
        {
            if (Exists(path))
            {
                CheckIsDirectory(path);
                return false;
            }

            CreateDirectory(path);
            return true;
        }

        public void CreateDirectory(string parentPath, string dirName)
        {
            CheckValidFileName(dirName);
            CheckExists(parentPath);
            CheckIsDirectory(parentPath);

            var path = Path.Combine(parentPath, dirName);
            CreateDirectory(path);
        }

        public void CreateResourceElement(ResourceType resourceType, string elementId, string description)
        {
            CheckValidFileName(elementId);

            CreateDirectoryIfNotExists(GetResourcePath(resourceType));

            var doneOnFilesystem = false;
            var paths = GetResourceElementPaths(resourceType, elementId);

            switch (resourceType)
            {
                case ResourceType.Map:
                    CreateMapDataFileIfNotExists(elementId);
                    break;

                case ResourceType.Item:
                case ResourceType.Sprite:
                case ResourceType.Enemy:
                case ResourceType.Entity:
                    foreach (var path in paths)
                        doneOnFilesystem |= CreateFileIfNotExists(path);
                    break;

                case ResourceType.Tileset:
                    doneOnFilesystem |= CreateFileIfNotExists(GetTilesetDataFilePath(elementId));
                    break;

                case ResourceType.Language:
                    doneOnFilesystem |= CreateDirectoryIfNotExists(GetLanguagePath(elementId));
                    doneOnFilesystem |= CreateDirectoryIfNotExists(GetLanguageImagesPath(elementId));
                    doneOnFilesystem |= CreateDirectoryIfNotExists(GetLanguageTextPath(elementId));
                    doneOnFilesystem |= CreateFileIfNotExists(GetDialogsPath(elementId));
                    doneOnFilesystem |= CreateFileIfNotExists(GetStringsPath(elementId));
                    break;

                case ResourceType.Music:
                case ResourceType.Sound:
                case ResourceType.Font:
                    break;
            }

            bool doneInResourceList = false;
            if (!Resources.Exists(resourceType, elementId))
            {
                doneInResourceList = true;
                Resources.Add(resourceType, elementId, description);
                Resources.Save();
            }

            if (!doneOnFilesystem && !doneInResourceList)
                throw new Exception("Resource '{0}' already exists".F(elementId));
        }
        #endregion
    }
}
