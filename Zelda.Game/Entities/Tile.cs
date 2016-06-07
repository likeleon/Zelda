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

        internal Tile(TileInfo tileInfo)
            : base(null, 0, tileInfo.Layer, tileInfo.Box.XY, tileInfo.Box.Size)
        {
            TilePatternId = tileInfo.PatternId;
            Pattern = tileInfo.Pattern;
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

    public class TileData : EntityData
    {
        public override EntityType Type => EntityType.Tile;
        public int Width { get; set; }
        public int Height { get; set; }
        public string Pattern { get; set; }

        internal override void CreateEntity(Map map)
        {
            var pattern = map.Tileset.GetTilePattern(Pattern);
            var size = EntityCreationCheckSize(Width, Height);

            for (int y = XY.Y; y < XY.Y + size.Height; y += pattern.Height)
                for (int x = XY.X; x < XY.X + size.Width; x += pattern.Width)
                    map.Entities.AddTileInfo(new TileInfo(Layer, new Rectangle(new Point(x, y), pattern.Size), Pattern, pattern));
        }
    }
}
