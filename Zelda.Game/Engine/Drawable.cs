using System.Drawing;

namespace Zelda.Game.Engine
{
    abstract class Drawable
    {
        public Point XY { get; set; }

        protected Drawable()
        {
        }

        public void Draw(Surface dstSurface)
        {
            Draw(dstSurface, new Point(0, 0));
        }

        public void Draw(Surface dstSurface, int x, int y)
        {
            Draw(dstSurface, new Point(x, y));
        }

        public void Draw(Surface dstSurface, Point dstPosition)
        {
            RawDraw(dstSurface, new Point(dstPosition.X + XY.X, dstPosition.Y + XY.Y));
        }

        public void DrawRegion(Rectangle region, Surface dstSurface)
        {
            DrawRegion(region, dstSurface, new Point(0, 0));
        }

        public void DrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            RawDrawRegion(region, dstSurface, new Point(dstPosition.X + XY.X, dstPosition.Y + XY.Y));
        }

        public abstract void RawDraw(Surface dstSurface, Point dstPosition);
        public abstract void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition);

        public virtual void Update()
        {
        }
    }
}
