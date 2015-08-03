using Caliburn.Micro;
using System;
using System.ComponentModel.Composition;
using System.IO;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    [Export(typeof(IMod))]
    class Mod : PropertyChangedBase, IMod
    {
        public event EventHandler Loaded;
        public event EventHandler Unloaded;

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
            RootDirectory = ModFileFactory.Create(this, rootPath);

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

            IsLoaded = false;
            if (Unloaded != null)
                Unloaded(this, EventArgs.Empty);
        }
    }
}
