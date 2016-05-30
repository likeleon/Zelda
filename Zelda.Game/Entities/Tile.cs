using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Tile : Entity
    {
        public override EntityType Type => EntityType.Tile;

        internal bool IsAnimated => Pattern.IsAnimated;
        internal string TilePatternId { get; }
        internal TilePattern Pattern { get; }
        internal override bool IsDrawnAtItsPosition => Pattern.IsDrawnAtItsPosition;

        internal Tile(Layer layer, Point xy, Size size, Tileset tileset, string tilePatternId)
            : base("", 0, layer, xy, size)
        {
            TilePatternId = tilePatternId;
            Pattern = tileset.GetTilePattern(tilePatternId);
        }

        internal void Draw(Surface dstSurface, Point viewport)
        {
            var dstPosition = new Rectangle(TopLeftX - viewport.X, TopLeftY - viewport.Y, Width, Height);
            Pattern.FillSurface(dstSurface, dstPosition, Map.Tileset, viewport);
        }

        internal override void DrawOnMap()
        {
            // 애니메이션되지 않는 타일들은 맵 로딩 시점에 한번만 그려지는 것을 기억해야 합니다.
            // 애니메이션되는 타일들만 이 함수가 호출됩니다
            Draw(Map.VisibleSurface, Map.CameraPosition.XY);
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
            Height = xmlData.Height.CheckField("Height");
            Pattern = xmlData.Pattern.CheckField("Pattern");
        }

        protected override EntityXmlData ExportXmlData()
        {
            return new TileXmlData()
            {
                Width = Width,
                Height = Height,
                Pattern = Pattern
            };
        }
    }

    public class TileXmlData : EntityXmlData
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Pattern { get; set; }
    }
}
