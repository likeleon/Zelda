using System;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    [XmlRoot("Mod")]
    public class ModProperties
    {
        // 모드가 호환되는 Zelda 버전
        [XmlElement]
        public string ZeldaVersion { get; set; }

        [XmlElement("WriteDir")]
        public string ModWriteDir { get; set; }

        [XmlElement]
        public string TitleBar { get; set; }

        [XmlElement("NormalModSize")]
        public string NormalModSizeString { get; set; }

        [XmlIgnore]
        internal Size NormalModSize { get; set; }

        [XmlElement("MinModSize")]
        public string MinModSizeString { get; set; }

        [XmlIgnore]
        internal Size MinModSize { get; set; }

        [XmlElement("MaxModSize")]
        public string MaxModSizeString { get; set; }

        [XmlIgnore]
        internal Size MaxModSize { get; set; }

        static Size ParseSize(string size_string)
        {
            string[] words = size_string.Split('x');
            if (words == null || words.Length < 2)
                throw new ArgumentException("Invalid size string format: " + size_string, "size_string");
            
            return new Size(int.Parse(words[0]), int.Parse(words[1]));
        }

        public static ModProperties ImportFrom(string fileName)
        {
            try
            {
                ModProperties properties = null;
                using (Stream buffer = ModFiles.DataFileRead(fileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ModProperties));
                    properties = (ModProperties)serializer.Deserialize(buffer);
                }

                if (properties.NormalModSizeString == null)
                    properties.NormalModSizeString = "320x240";
                if (properties.MinModSizeString == null)
                    properties.MinModSizeString = properties.NormalModSizeString;
                if (properties.MaxModSizeString == null)
                    properties.MaxModSizeString = properties.NormalModSizeString;

                properties.NormalModSize = ParseSize(properties.NormalModSizeString);
                properties.MinModSize = ParseSize(properties.MinModSizeString);
                properties.MaxModSize = ParseSize(properties.MaxModSizeString);

                if (properties.NormalModSize.Width < properties.MinModSize.Width ||
                    properties.NormalModSize.Height < properties.MinModSize.Height ||
                    properties.NormalModSize.Width > properties.MaxModSize.Width ||
                    properties.NormalModSize.Height > properties.MaxModSize.Height)
                    throw new Exception("Invalid range of mod sizes");

                return properties;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to load Mod.xml: " + ex.Message);
            }
        }
    }
}
