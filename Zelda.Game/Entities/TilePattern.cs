using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    abstract class TilePattern
    {
        readonly Ground _ground;
        public Ground Ground
        {
            get { return _ground; }
        }

        readonly Size _size;
        public Size Size
        {
            get { return _size; }
        }

        public int Width
        {
            get { return _size.Width; }
        }

        public int Height
        {
            get { return _size.Height; }
        }

        public virtual bool IsAnimated
        {
            get { return true; }
        }

        // 뷰포트에 들어있지 않더라도 그려지길 원한다면 false를 리턴하도록 합니다
        // 대표적으로 parallax 스크롤링와 같이 잔상을 필요로 할 경우 사용할 수 있습니다.
        public virtual bool IsDrawnAtItsPosition
        {
            get { return true; }
        }

        public abstract void Draw(Surface dstSurface, Point dstPosition, Tileset tileset, Point viewport);

        protected TilePattern(Ground ground, Size size)
        {
            _ground = ground;
            _size = size;

            if (size.Width <= 0 || size.Height <= 0 ||
                size.Width % 8 != 0 || size.Height % 8 != 0)
            {
                string msg = "Invalid tile pattern: the size is ({0}x{1})"
                    + " but should be positive and multiple of 8 pixels"
                    .F(size.Width, size.Height);
                Debug.Die(msg);
            }

            if (ground.IsGroundDiagonal())
            {
                Debug.CheckAssertion(size.IsSquare,
                    "Invalid tile pattern: a tile pattern with a diagonal wall must be squre");
            }
        }

        // 영역을 이 타일 패턴을 사용해서 채웁니다
        public void FillSurface(Surface dstSurface, Rectangle dstPosition, Tileset tileset, Point viewport)
        {
            Point dst = new Point();

            int limitX = dstPosition.X + dstPosition.Width;
            int limitY = dstPosition.Y + dstPosition.Height;

            for (int y = dstPosition.Y; y < limitY; y += Height)
            {
                if ((y <= dstSurface.Height && y + Height > 0) || !IsDrawnAtItsPosition)
                {
                    dst = new Point(dst.X, y);

                    for (int x = dstPosition.X; x < limitX; x += Width)
                    {
                        if ((x <= dstSurface.Width && x + Width > 0) || !IsDrawnAtItsPosition)
                        {
                            dst = new Point(x, dst.Y);
                            Draw(dstSurface, dst, tileset, viewport);
                        }
                    }
                }
            }
        }

        public static void Update()
        {
            AnimatedTilePattern.Update();
            TimeScrollingTilePattern.Update();
        }
    }
}
