using System;
using System.ComponentModel.Composition;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.Mods.Services
{
    [Export(typeof(IModService))]
    class ModService : IModService
    {
        public bool IsLoaded { get { return Mod != null; } }

        public IMod Mod { get; private set; }

        public event EventHandler<IMod> Loaded;
        public event EventHandler<IMod> Unloaded;

        public void Load(string rootPath)
        {
            if (Mod != null)
                throw new InvalidOperationException("Mod already loaded");

            var modPath = new ModPath(rootPath);
            if (!modPath.Exists(modPath.PropertiesPath))
                throw new Exception("No mod was found in directory\n'{0}'".F(modPath));

            var modProperties = new Models.ModProperties(modPath);
            CheckVersion(modProperties);

            Mod = new Models.Mod(modProperties);

            if (Loaded != null)
                Loaded(this, Mod);
        }

        static void CheckVersion(Models.ModProperties properties)
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

            var unloadedMod = Mod;
            Mod = null;
            if (Unloaded != null)
                Unloaded(this, unloadedMod);
        }
    }
}
