using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class DynamicTile : Entity
    {
        public override EntityType Type => EntityType.DynamicTile;
        public override Ground ModifiedGround => _tilePattern.Ground;
        internal override bool IsGroundModifier => true;

        public string TilePatternId { get; }

        readonly TilePattern _tilePattern;

        internal DynamicTile(DynamicTileData data, Size size, Tileset tileset)
            : base(data.Name, Direction4.Right, data.Layer, data.XY, size)
        {
            TilePatternId = data.Pattern;
            _tilePattern = tileset.GetTilePattern(TilePatternId);
            SetEnabled(data.EnabledAtStart);
        }

        internal override void DrawOnMap()
        {
            var cameraPosition = Map.CameraPosition;
            var dstPosition = new Rectangle(TopLeftX - cameraPosition.X, TopLeftY - cameraPosition.Y,Width, Height);
            _tilePattern.FillSurface(Map.VisibleSurface, dstPosition, Map.Tileset, cameraPosition.XY);
        }
    }

    public class DynamicTileData : EntityData
    {
        public override EntityType Type => EntityType.DynamicTile;

        public string Pattern { get; set; }
        public int Width { get; set;  }
        public int Height { get; set; }
        public bool EnabledAtStart { get; set; }

        internal override void CreateEntity(Map map)
        {
            map.Entities.AddEntity(new DynamicTile(this, EntityCreationCheckSize(Width, Height), map.Tileset));
        }
    }
}
