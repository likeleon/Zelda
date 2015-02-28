﻿using Zelda.Game.Engine;
using RawSprite = Zelda.Game.Sprite;

namespace Zelda.Game.Script
{
    public class Sprite : Drawable
    {
        readonly RawSprite _rawSprite;

        public static Sprite Create(string imageFileName)
        {
            RawSprite rawSprite = new RawSprite(imageFileName);
            return new Sprite(rawSprite);
        }

        Sprite(RawSprite rawSprite)
            : base(rawSprite)
        {
            _rawSprite = rawSprite;
        }
    }
}
