using System.Drawing;
using Zelda.Game.Movements;

namespace Zelda.Game.Engine
{
    abstract class Drawable
    {
        public Point XY { get; set; }

        Movement _movement;
        public Movement Movement
        {
            get { return _movement; }
        }

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
            RawDraw(dstSurface, dstPosition + XY);
        }

        public void DrawRegion(Rectangle region, Surface dstSurface)
        {
            DrawRegion(region, dstSurface, new Point(0, 0));
        }

        public void DrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            RawDrawRegion(region, dstSurface, dstPosition + XY);
        }

        public abstract void RawDraw(Surface dstSurface, Point dstPosition);
        public abstract void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition);

        public virtual void Update()
        {
            if (_movement != null)
            {
                _movement.Update();
                if (_movement != null && _movement.IsFinished)
                    StopMovement();
            }
        }

        public void StartMovement(Movement movement)
        {
            StopMovement();
            _movement = movement;
            movement.SetDrawable(this);
        }

        public void StopMovement()
        {
            _movement = null;
        }
    }
}
