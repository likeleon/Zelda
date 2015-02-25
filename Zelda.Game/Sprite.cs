using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Sprite : Drawable
    {
        readonly Surface _srcImage;

        public static void Initialize()
        {
        }

        public static void Quit()
        {
        }

        public Sprite(string imageFileName, Size size)
        {
            _srcImage = Surface.Create(imageFileName, size);
            if (_srcImage == null)
                throw new ArgumentException("Cannot load image '" + imageFileName + "'");
        }

        public override void RawDraw(Surface dstSurface, Point dstPosition)
        {
            Rectangle rect = new Rectangle(_srcImage.Size);
            _srcImage.DrawRegion(rect, dstSurface, dstPosition);
        }

        public override void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            throw new NotImplementedException();
        }
    }
}
