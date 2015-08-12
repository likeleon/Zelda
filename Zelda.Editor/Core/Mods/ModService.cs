using System;
using System.ComponentModel.Composition;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    [Export(typeof(IModService))]
    class ModService : IModService
    {
        public bool IsLoaded { get { return Mod != null; } }

        public IMod Mod { get; private set; }

        public event EventHandler Loaded;
        public event EventHandler Unloaded;

        public void Load(string rootPath)
        {
            if (Mod != null)
                throw new InvalidOperationException("Mod already loaded");

            var modPath = new ModPath(rootPath);
            if (!modPath.Exists(modPath.PropertiesPath))
                throw new Exception("No mod was found in directory\n'{0}'".F(modPath));

            var modProperties = new ModProperties(modPath);
            CheckVersion(modProperties);

            Mod = new Mod(rootPath);

            if (Loaded != null)
                Loaded(this, EventArgs.Empty);
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
            if (Mod == null)
                throw new InvalidOperationException("Mod not loaded");

            Mod = null;
            if (Unloaded != null)
                Unloaded(this, EventArgs.Empty);
        }
    }
}
