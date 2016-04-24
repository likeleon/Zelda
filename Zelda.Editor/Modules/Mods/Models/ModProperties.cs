using System;
using Zelda.Game;
using Zelda.Game.Lowlevel;

namespace Zelda.Editor.Modules.Mods.Models
{
    class ModProperties
    {
        readonly Game.ModProperties _properties = new Game.ModProperties();

        public ModPath ModPath { get; private set; }
        public string ZeldaVersion { get { return _properties.ZeldaVersion; } }
        public string WriteDir { get { return _properties.ModWriteDir; } }
        public Size NormalModSize { get { return _properties.NormalModSize; } } 

        public ModProperties(ModPath modPath)
        {
            ModPath = modPath;
            Reload();
        }

        void Reload()
        {
            string fileName = ModPath.PropertiesPath;
            if (!ModPath.Exists(fileName))
                return;

            if (!_properties.ImportFromFile(fileName))
                throw new Exception("Cannot open file '{0}'".F(fileName));
        }

    }
}
