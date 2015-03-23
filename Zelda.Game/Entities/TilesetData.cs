using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class TilesetData : XmlData
    {
        [Description("타일셋의 배경색")]
        public Color BackgroundColor { get; private set; }

        public TilesetData()
        {
            BackgroundColor = Color.White;
        }
     
        protected override bool ImportFromStream(Stream stream)
        {
            TilesetXmlData data = stream.XmlDeserialize<TilesetXmlData>();
            BackgroundColor = data.BackgroundColor.CheckColor("BackgroundColor");
            return true;
        }
    }

    [XmlRoot("Tileset")]
    public class TilesetXmlData
    {
        public ColorXmlData BackgroundColor { get; set; }
    }
}
