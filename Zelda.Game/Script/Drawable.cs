using RawDrawable = Zelda.Game.Engine.Drawable;

namespace Zelda.Game.Script
{
    public abstract class Drawable
    {
        readonly RawDrawable _rawDrawable;

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
    }
}
