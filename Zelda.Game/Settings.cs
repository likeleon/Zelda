using System;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Settings
    {
        public static bool Load(string fileName)
        {
            Debug.CheckAssertion(!String.IsNullOrEmpty(ModFiles.ModWriteDir),
                "Cannot load setings: no mod write directory was specified in mod.xml");

            if (!ModFiles.DataFileExists(fileName))
                return false;

            var settings = ModFiles.DataFileRead(fileName).XmlDeserialize<SettingsXmlData>();

            var language = settings.Language;
            if (!language.IsNullOrEmpty() && CurrentMod.HasLanguage(language))
                CurrentMod.SetLanguage(language);

            return true;
        }

        public static bool Save(string fileName)
        {
            Debug.CheckAssertion(!String.IsNullOrEmpty(ModFiles.ModWriteDir),
                "Cannot save settings: no mod write directory was specified in mod.xml");

            var settings = new SettingsXmlData();
            if (!CurrentMod.Language.IsNullOrEmpty())
                settings.Language = CurrentMod.Language;
            
            var stream = new MemoryStream();
            settings.XmlSerialize(stream);
            ModFiles.DataFileSave(fileName, stream.GetBuffer(), stream.Length);
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
