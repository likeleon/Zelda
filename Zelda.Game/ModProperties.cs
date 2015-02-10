using System.ComponentModel;
using System.Xml.Serialization;

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
    }
}
