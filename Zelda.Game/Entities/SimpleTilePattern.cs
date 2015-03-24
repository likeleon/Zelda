using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class SimpleTilePattern : TilePattern
    {
        protected readonly Rectangle _positionInTileset;

        public override bool IsAnimated
        {
            get { return false; }
        }
     
        public SimpleTilePattern(Ground ground, Point xy, Size size)
            : base(ground, size)
        {
            _positionInTileset = new Rectangle(xy, size);
        }

        public override void Draw(Surface dstSurface, Point dstPosition, Tileset tileset, Point viewport)
        {
            Surface tilesetImage = tileset.TilesImage;
            tilesetImage.DrawRegion(_positionInTileset, dstSurface, dstPosition);
        }
    }
}
