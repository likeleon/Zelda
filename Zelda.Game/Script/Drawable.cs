using Zelda.Game.Engine;
using RawDrawable = Zelda.Game.Engine.Drawable;

namespace Zelda.Game.Script
{
    public abstract class Drawable
    {
        readonly RawDrawable _rawDrawable;
        internal RawDrawable RawDrawable
        {
            get { return _rawDrawable; }
        }

        public Point XY
        {
            get { return _rawDrawable.XY; }
            set { _rawDrawable.XY = value; }
        }

        internal Drawable(RawDrawable rawDrawable)
        {
            _rawDrawable = rawDrawable;
        }

        public void Draw(Surface dstSurface)
        {
            Draw(dstSurface, 0, 0);
        }

        public void Draw(Surface dstSurface, int x, int y)
        {
            _rawDrawable.Draw(dstSurface.RawSurface, x, y);
        }

        public void StopMovement()
        {
            _rawDrawable.StopMovement();
        }
    }
}
