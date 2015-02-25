using Zelda.Game.Engine;
using RawSprite = Zelda.Game.Sprite;

namespace Zelda.Game.Script
{
    public class Sprite : Drawable
    {
        readonly RawSprite _rawSprite;

        public static Sprite Create(string imageFileName, int width, int height)
        {
            RawSprite rawSprite = new RawSprite(imageFileName, new Size(width, height));
            return new Sprite(rawSprite);
        }

        Sprite(RawSprite rawSprite)
            : base(rawSprite)
        {
            _rawSprite = rawSprite;
        }
    }
}
