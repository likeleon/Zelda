using System;
using System.Xml.Serialization;
using Zelda.Game.Lowlevel;

namespace Zelda.Game
{
    public class ModProperties : XmlData
    {
        public string ZeldaVersion { get; set; }
        public string ModWriteDir { get; set; }
        public string TitleBar { get; set; }
        public Size NormalModSize { get; set; }
        public Size MinModSize { get; set; }
        public Size MaxModSize { get; set; }

        protected override bool OnImportFromBuffer(byte[] buffer)
        {
            try
            {
                var data = buffer.XmlDeserialize<ModPropertiesXmlData>();
                ZeldaVersion = data.ZeldaVersion;
                ModWriteDir = data.ModWriteDir;
                TitleBar = data.TitleBar;

                NormalModSize = new Size(320, 240);
                MinModSize = data.MinModSizeString?.ToSize() ?? NormalModSize;
                MaxModSize = data.MaxModSizeString?.ToSize() ?? NormalModSize;

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
