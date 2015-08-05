using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    [Export(typeof(IMod))]
    class Mod : PropertyChangedBase, IMod
    {
        public event EventHandler Loaded;
        public event EventHandler Unloaded;

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

        string _rootPath;
        string _name;
        bool _isLoaded;
        IModFile _rootDirectory;

        public string RootPath
        {
            get { return _rootPath; }
            set { this.SetProperty(ref _rootPath, value); }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { this.SetProperty(ref _isLoaded, value); }
        }

        public string Name
        {
            get { return _name; }
            set { this.SetProperty(ref _name, value); }
        }

        public IModFile RootDirectory
        {
            get { return _rootDirectory; }
            set { this.SetProperty(ref _rootDirectory, value); }
        }

        public List<IModFile> RootDirectories { get; private set; }

        public Mod()
        {
            RootPath = string.Empty;
        }

        public void Load(string rootPath)
        {
            if (IsLoaded)
                Unload();

            var modPath = new ModPath(rootPath);
            if (!modPath.Exists(modPath.PropertiesPath))
                throw new Exception("No mod was found in directory\n'{0}'".F(modPath));

            var modProperties = new ModProperties(modPath);
            CheckVersion(modProperties);

            RootPath = rootPath;
            Name = Path.GetFileName(rootPath);
            RootDirectory = ModFileBuilder.Build(this);
            RootDirectories = new List<IModFile>() { RootDirectory };
            NotifyOfPropertyChange("RootDirectories");

            IsLoaded = true;
            if (Loaded != null)
                Loaded(this, EventArgs.Empty);
        }

        static bool ModExists(string rootPath)
        {
            var modPath = new ModPath(rootPath);
            return modPath.Exists(modPath.PropertiesPath);
        }

        static void CheckVersion(ModProperties properties)
        {
            Version modVersion = null;
            if (!Version.TryParse(properties.ZeldaVersion, out modVersion))
                throw new Exception("Missing Zelda verion in '{0}'".F(properties.ModPath.PropertiesPath));

            Version editorVersion = null;
            if (!Version.TryParse(Properties.Settings.Default.ZeldaVersion, out editorVersion))
                throw new Exception("Invalid editor zelda verion: '{0}'".F(Properties.Settings.Default.ZeldaVersion));

            if (modVersion.Major > editorVersion.Major ||
                (modVersion.Major == editorVersion.Major && modVersion.Minor > editorVersion.Minor))
                ThrowObsoleteEditorException(modVersion.ToString());
            else if (modVersion.Major < editorVersion.Major ||
                (modVersion.Major == editorVersion.Major && modVersion.Minor < editorVersion.Minor))
                ThrowObsoleteEditorException(modVersion.ToString());
        }

        static void ThrowObsoleteEditorException(string modFormat)
        {
            string msg = "The format of this mod ({0}) is not supported by this version of the editor ({1})".
                F(modFormat, Properties.Settings.Default.ZeldaVersion);
            throw new Exception(msg);
        }

        public void Unload()
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Mod not loaded");

            RootPath = null;
            Name = null;
            RootDirectory = null;
            RootDirectories = null;
            NotifyOfPropertyChange("RootDirectories");

            IsLoaded = false;
            if (Unloaded != null)
                Unloaded(this, EventArgs.Empty);
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

            // TODO: resources.Exists()로 리소스 리스트에 포함되어 있는지 확인
            return true;
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
            var pathFromResource = path.Substring(resourceDir.Length + 2);
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
