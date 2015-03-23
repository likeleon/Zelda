using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class MapData : XmlData
    {
        [Description("맵의 크기, 픽셀 단위")]
        public Size Size { get; private set; }

        [Description("맵의 월드")]
        public string World { get; private set; }

        [Description("월드 상에서의 이 맵의 좌측 최상단 좌표")]
        public Point Location { get; private set; }

        [Description("층")]
        public int Floor { get; private set; }

        [Description("타일 셋")]
        public string TilesetId { get; private set; }

        public static readonly int NoFloor = 9999;

        public MapData()
        {
            World = String.Empty;
        }

        protected override bool ImportFromStream(Stream stream)
        {
            try
            {
                MapXmlData data = stream.XmlDeserialize<MapXmlData>();
                int x = data.Properties.X.OptField(0);
                int y = data.Properties.Y.OptField(0);
                int width = data.Properties.Width.CheckField("Width");
                int height = data.Properties.Height.CheckField("Height");
                string world = data.Properties.World.OptField(String.Empty);
                int floor = data.Properties.Floor.OptField(MapData.NoFloor);
                string tilesetId = data.Properties.Tileset.CheckField("Tileset");

                Location = new Point(x, y);
                Size = new Size(width, height);
                World = world;
                Floor = floor;
                TilesetId = tilesetId;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load map: {0}".F(ex.Message));
                return false;
            }
            return true;
        }
    }

    [XmlRoot("MapData")]
    public class MapXmlData
    {
        public class PropertiesData
        {
            public int? X { get; set; }
            public int? Y { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public string World { get; set; }
            public int? Floor { get; set; }
            public string Tileset { get; set; }
        }

        public PropertiesData Properties { get; set; }
    }
}
