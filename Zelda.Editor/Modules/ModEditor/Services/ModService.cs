using Caliburn.Micro;
using System;
using System.ComponentModel.Composition;
using Zelda.Game;

namespace Zelda.Editor.Modules.ModEditor.Services
{
    [Export(typeof(IModService))]
    class ModService : PropertyChangedBase, IModService
    {
        public event EventHandler ModLoaded;
        public event EventHandler ModUnloaded;

        public bool IsLoaded { get; private set; }
        public string RootPath { get; private set; }

        public ModService()
        {
            RootPath = string.Empty;
        }

        public void LoadMod(string rootPath)
        {
            if (IsLoaded)
                UnloadMod();

            var modPath = new ModPath(rootPath);
            if (!modPath.Exists(modPath.PropertiesPath))
                throw new Exception("No mod was found in directory\n'{0}'".F(modPath));

            var modProperties = new ModProperties(modPath);
            CheckVersion(modProperties);

            RootPath = rootPath;

            if (ModLoaded != null)
                ModLoaded(this, EventArgs.Empty);
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

        public void UnloadMod()
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Mod not loaded");

            RootPath = string.Empty;

            if (ModUnloaded != null)
                ModUnloaded(this, EventArgs.Empty);
        }
    }
}
