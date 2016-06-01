using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Zelda.Editor.Modules.Mods.Models
{
    class ModProperties
    {

        public ModPath ModPath { get; }
        public string ZeldaVersion => _properties.ZeldaVersion;
        public string WriteDir => _properties.ModWriteDir;
        public Size NormalModSize => _properties.NormalModSize;

        Game.ModProperties _properties = new Game.ModProperties();

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

            _properties = XmlLoader.Load<Game.ModProperties>(fileName);
        }

    }
}
