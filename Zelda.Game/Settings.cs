using System;
using System.IO;
using System.Xml.Serialization;

namespace Zelda.Game
{
    class Settings
    {
        public static bool Load(string fileName)
        {
            var modFiles = MainLoop.Mod.ModFiles;

            Debug.CheckAssertion(!modFiles.ModWriteDir.IsNullOrWhiteSpace(),
                "Cannot load setings: no mod write directory was specified in mod.xml");

            if (!modFiles.DataFileExists(fileName))
                return false;

            var settings = modFiles.DataFileRead(fileName).XmlDeserialize<SettingsXmlData>();

            var language = settings.Language;
            if (!language.IsNullOrEmpty() && MainLoop.Mod.HasLanguage(language))
                MainLoop.Mod.SetLanguage(language);

            return true;
        }

        public static bool Save(string fileName)
        {
            var modFiles = MainLoop.Mod.ModFiles;

            Debug.CheckAssertion(!String.IsNullOrEmpty(modFiles.ModWriteDir),
                "Cannot save settings: no mod write directory was specified in mod.xml");

            var settings = new SettingsXmlData();
            if (!MainLoop.Mod.Language.IsNullOrEmpty())
                settings.Language = MainLoop.Mod.Language;
            
            var stream = new MemoryStream();
            settings.XmlSerialize(stream);
            modFiles.DataFileSave(fileName, stream.GetBuffer(), stream.Length);
            return true;
        }
    }

    [XmlRoot("Settings")]
    public class SettingsXmlData
    {
        public string VideoMode { get; set; }
        public bool? Fullscreen { get; set; }
        public int? SoundVolume { get; set; }
        public int? MusicVolume { get; set; }
        public string Language { get; set; }
        public bool JoypadEnabled { get; set; }
    }
}
