using System;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    public class ModProperties : XmlData
    {
        public string ZeldaVersion { get; set; }
        public string ModWriteDir { get; set; }
        public string TitleBar { get; set; }
        internal Size NormalModSize { get; set; }
        internal Size MinModSize { get; set; }
        internal Size MaxModSize { get; set; }

        protected override bool ImportFromBuffer(byte[] buffer)
        {
            try
            {
                var data = buffer.XmlDeserialize<ModPropertiesXmlData>();
                ZeldaVersion = data.ZeldaVersion;
                ModWriteDir = data.ModWriteDir;
                TitleBar = data.TitleBar;

                var normalModSizeString = data.NormalModSizeString ?? "320x240";
                NormalModSize = Video.ParseSize(normalModSizeString);
                MinModSize = Video.ParseSize(data.MinModSizeString ?? normalModSizeString);
                MaxModSize = Video.ParseSize(data.MaxModSizeString ?? normalModSizeString);

                if (NormalModSize.Width < MinModSize.Width || NormalModSize.Height < MinModSize.Height ||
                    NormalModSize.Width > MaxModSize.Width || NormalModSize.Height > MaxModSize.Height)
                    throw new Exception("Invalid range of mod sizes");

                return true;
            }
            catch (Exception ex)
            {
                Debug.Die("Failed to load mod.xml: " + ex.Message);
                return false;
            }
        }

        [XmlRoot("Mod")]
        public class ModPropertiesXmlData
        {
            [XmlElement]
            public string ZeldaVersion { get; set; }

            [XmlElement("WriteDir")]
            public string ModWriteDir { get; set; }

            [XmlElement]
            public string TitleBar { get; set; }

            [XmlElement("NormalModSize")]
            public string NormalModSizeString { get; set; }

            [XmlElement("MinModSize")]
            public string MinModSizeString { get; set; }

            [XmlElement("MaxModSize")]
            public string MaxModSizeString { get; set; }
        }
    }
}
