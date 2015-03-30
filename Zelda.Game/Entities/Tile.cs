﻿using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class Tile : MapEntity
    {
        public override EntityType Type
        {
            get { return EntityType.Tile; }
        }

        public Tile(Layer layer, Point xy, Size size, Tileset tileset, string tilePatternId)
            : base("", 0, layer, xy, size)
        {
        }
    }

    class TileData : EntityData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Pattern { get; set; }

        public TileData(TileXmlData xmlData)
            : base(EntityType.Tile, xmlData)
        {
            Width = xmlData.Width.CheckField("Width");
            Height = xmlData.Width.CheckField("Height");
            Pattern = xmlData.Pattern.CheckField("Pattern");
        }
    }

    public class TileXmlData : EntityXmlData
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Pattern { get; set; }
    }
}
