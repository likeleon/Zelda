using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    class SelfScrollingTilePattern : SimpleTilePattern
    {
        public override bool IsAnimated
        {
            get { return true; }
        }

        public SelfScrollingTilePattern(Ground ground, Point xy, Size size)
            : base(ground, xy, size)
        {
        }

        public override void Draw(Surface dstSurface, Point dstPosition, Tileset tileset, Point viewport)
        {
            int offsetX = 0;
            if (dstPosition.X >= 0)
                offsetX = dstPosition.X % _positionInTileset.Width;
            else
                offsetX = _positionInTileset.Width - (-dstPosition.X % _positionInTileset.Width);

            int offsetY = 0;
            if (dstPosition.Y >= 0)
                offsetY = dstPosition.Y % _positionInTileset.Height;
            else
                offsetY = _positionInTileset.Height - (-dstPosition.Y % _positionInTileset.Height);

            // 스크롤 ratio 적용
            offsetX /= 2;
            offsetY /= 2;

            Surface tilesetImage = tileset.TilesImage;

            Rectangle src = new Rectangle(
                _positionInTileset.X + offsetX,
                _positionInTileset.Y + offsetY,
                _positionInTileset.Width - offsetX,
                _positionInTileset.Height - offsetY);
            Point dst = dstPosition;
            tilesetImage.DrawRegion(src, dstSurface, dst);

            src = new Rectangle(
                _positionInTileset.X,
                _positionInTileset.Y + offsetY,
                offsetX,
                _positionInTileset.Height - offsetY);
            dst = new Point(
                dstPosition.X + (_positionInTileset.Width - offsetX), 
                dstPosition.Y);
            tilesetImage.DrawRegion(src, dstSurface, dst);

            src = new Rectangle(
                _positionInTileset.X + offsetX,
                _positionInTileset.Y,
                _positionInTileset.Width - offsetX,
                offsetY);
            dst = new Point(
                dstPosition.X,
                dstPosition.Y + (_positionInTileset.Height - offsetY));
            tilesetImage.DrawRegion(src, dstSurface, dst);

            src = new Rectangle(
                _positionInTileset.X,
                _positionInTileset.Y,
                offsetX,
                offsetY);
            dst = new Point(
                dstPosition.X + (_positionInTileset.Width - offsetX),
                dstPosition.Y + (_positionInTileset.Height - offsetY));
            tilesetImage.DrawRegion(src, dstSurface, dst);
        }
    }
}
