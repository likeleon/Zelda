using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    class DynamicTile : MapEntity
    {
        readonly string _tilePatternId;
        readonly TilePattern _tilePattern;
        readonly ScriptDynamicTile _scriptDynamicTile;

        public override EntityType Type { get { return EntityType.DynamicTile; } }
        public override ScriptEntity ScriptEntity { get { return _scriptDynamicTile; } }
        // TODO: IsGroundModifier
        // TODO: GetModifiedGround

        public DynamicTile(string name, Layer layer, Point xy, Size size, Tileset tileset, string tilePatternId, bool enabled)
            : base(name, Direction4.Right, layer, xy, size)
        {
            _tilePatternId = tilePatternId;
            _tilePattern = tileset.GetTilePattern(tilePatternId);
            SetEnabled(enabled);
            _scriptDynamicTile = new ScriptDynamicTile(this);
        }

        public override void DrawOnMap()
        {
            if (!IsDrawn())
                return;

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
    }

    public class DynamicTileXmlData : EntityXmlData
    {
        public string Pattern { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? EnabledAtStart { get; set; }
    }
}
