using Caliburn.Micro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class Mod : PropertyChangedBase, IMod
    {
        static readonly Dictionary<ResourceType, string> _resourceDirs = new Dictionary<ResourceType, string>()
        {
            { ResourceType.Map,      "maps"      },
            { ResourceType.TileSet,  "tilesets"  },
            { ResourceType.Sprite,   "sprites"   },
            { ResourceType.Music,    "musics"    },
            { ResourceType.Sound,    "sounds"    },
            { ResourceType.Item,     "items"     },
            { ResourceType.Enemy,    "enemies"   },
            { ResourceType.Entity,   "entities"  },
            { ResourceType.Language, "languages" },
            { ResourceType.Font,     "fonts"     },
        };

        public string RootPath { get; private set; }

        public ModResources Resources { get; private set; }

        public string ResourceListPath { get { return Path.Combine(RootPath, "project_db.xml"); } }

        public string Name { get; private set; }

        public IModFile RootDirectory { get; private set; }

        public Mod(string rootPath)
        {
            RootPath = rootPath;
            Name = Path.GetFileName(rootPath);
            Resources = ModResources.Load(ResourceListPath);
            RootDirectory = ModFileBuilder.Build(this);
        }

        static bool ModExists(string rootPath)
        {
            var modPath = new ModPath(rootPath);
            return modPath.Exists(modPath.PropertiesPath);
        }
        
        public bool IsDirectory(string path)
        {
            return Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        bool Exists(string path)
        {
            return IsInRootPath(path) && (File.Exists(path) || Directory.Exists(path));
        }

        bool IsInRootPath(string path)
        {
            return path.StartsWith(RootPath);
        }

        public bool IsModRootDirectory(string path)
        {
            return path == RootPath;
        }

        public bool IsResourceDirectory(string path, ref ResourceType resourceType)
        {
            foreach (var type in _resourceDirs.Keys)
            {
                if (path == GetResourceDirectory(type))
                {
                    resourceType = type;
                    return true;
                }
            }
            return false;
        }

        string GetResourceDirectory(ResourceType resourceType)
        {
            var dirName = _resourceDirs[resourceType];
            return Path.Combine(RootPath, dirName);
        }

        public bool IsInResourceDirectory(string path, ref ResourceType resourceType)
        {
            foreach (var type in _resourceDirs.Keys)
            {
                if (path.StartsWith(GetResourceDirectory(type) + Path.DirectorySeparatorChar))
                {
                    resourceType = type;
                    return true;
                }
            }
            return false;
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
            var resourceDir = GetResourceDirectory(resourceType);
            var pathFromResource = path.Substring(resourceDir.Length + 1);
            var extensions = new List<string>();
            switch (resourceType)
            {
                case ResourceType.Map:
                case ResourceType.TileSet:
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
    }
}
