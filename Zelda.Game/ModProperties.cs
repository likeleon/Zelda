using System;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    [XmlRoot("Mod")]
    public class ModProperties : IXmlDeserialized
    {
        public string ZeldaVersion { get; set; }
        public string TitleBar { get; set; }

        [XmlElement("WriteDir")]
        public string ModWriteDir { get; set; }

        [XmlElement("NormalModSize")]
        public string NormalModSizeStr { get; set; } = DefaultModSize;

        [XmlElement("MinModSize")]
        public string MinModSizeStr { get; set; } = DefaultModSize;

        [XmlElement("MaxModSize")]
        public string MaxModSizeStr { get; set; } = DefaultModSize;

        [XmlIgnore] public Size NormalModSize { get; private set; }
        [XmlIgnore] public Size MinModSize { get; private set; }
        [XmlIgnore] public Size MaxModSize { get; private set; }

        static readonly string DefaultModSize = "320x240";

        public void OnDeserialized()
        {
            NormalModSize = NormalModSizeStr.ToSize();
            MinModSize = MinModSizeStr.ToSize();
            MaxModSize = MaxModSizeStr.ToSize();

            if (NormalModSize.Width < MinModSize.Width || 
                NormalModSize.Height < MinModSize.Height ||
                NormalModSize.Width > MaxModSize.Width || 
                NormalModSize.Height > MaxModSize.Height)
                throw new Exception("Invalid range of mod sizes");
        }
    }
}
