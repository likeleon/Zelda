using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class DynamicTile : Entity
    {
        public override EntityType Type => EntityType.DynamicTile;
        public override Ground ModifiedGround => _tilePattern.Ground;
        internal override bool IsGroundModifier => true;

        readonly string _tilePatternId;
        readonly TilePattern _tilePattern;

        internal DynamicTile(string name, Layer layer, Point xy, Size size, Tileset tileset, string tilePatternId, bool enabled)
            : base(name, Direction4.Right, layer, xy, size)
        {
            _tilePatternId = tilePatternId;
            _tilePattern = tileset.GetTilePattern(tilePatternId);
            SetEnabled(enabled);
        }

        internal override void DrawOnMap()
        {
            var cameraPosition = Map.CameraPosition;
            var dstPosition = new Rectangle(TopLeftX - cameraPosition.X, TopLeftY - cameraPosition.Y,Width, Height);
            _tilePattern.FillSurface(Map.VisibleSurface, dstPosition, Map.Tileset, cameraPosition.XY);
        }
    }

    class DynamicTileData : EntityData
    {
        public string Pattern { get; set; }
        public int Width { get; set;  }
        public int Height { get; set; }
        public bool EnabledAtStart { get; set; }

        public DynamicTileData(DynamicTileXmlData xmlData)
            : base(EntityType.DynamicTile, xmlData)
        {
            Pattern = xmlData.Pattern.CheckField("Pattern");
            Width = xmlData.Width.CheckField("Width");
            Height = xmlData.Height.CheckField("Height");
            EnabledAtStart = xmlData.EnabledAtStart.CheckField("EnabledAtStart");
        }

        protected override EntityXmlData ExportXmlData()
        {
            return new DynamicTileXmlData()
            {
                Pattern = Pattern,
                Width = Width,
                Height = Height,
                EnabledAtStart = EnabledAtStart
            };
        }
    }

    public class DynamicTileXmlData : EntityXmlData
    {
        public string Pattern { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? EnabledAtStart { get; set; }
    }
}
