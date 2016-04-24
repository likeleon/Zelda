using Zelda.Game.Lowlevel;

namespace Zelda.Game.Entities
{
    // 카메라가 2픽셀 이동할 때 1픽셀만 이동시켜 잔상을 연출할 때 사용합니다.
    class ParallaxScrollingTilePattern : SimpleTilePattern
    {
        public override bool IsAnimated
        {
            get { return true; }
        }

        public override bool IsDrawnAtItsPosition
        {
            get { return false; }
        }

        // 타일이 1픽셀 이동하기 위해 필요로 하는 뷰포트 거리
        public static readonly int Ratio = 2;

        public ParallaxScrollingTilePattern(Ground ground, Point xy, Size size)
            : base(ground, xy, size)
        {
        }

        public override void Draw(Surface dstSurface, Point dstPosition, Tileset tileset, Point viewport)
        {
            Surface tilesetImage = tileset.TilesImage;
            Point dst = dstPosition;
            dst += viewport / Ratio;
            tilesetImage.DrawRegion(_positionInTileset, dstSurface, dst);
        }
    }
}
