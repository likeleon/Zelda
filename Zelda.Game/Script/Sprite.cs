using System;
using Zelda.Game.Engine;
using RawSprite = Zelda.Game.Sprite;

namespace Zelda.Game.Script
{
    public class Sprite : Drawable
    {
        readonly RawSprite _rawSprite;

        public int Direction
        {
            get { return _rawSprite.CurrentDirection; }
        }

        public static Sprite Create(string animationSetId)
        {
            RawSprite rawSprite = new RawSprite(animationSetId);
            ScriptContext.AddDrawable(rawSprite);
            return new Sprite(rawSprite);
        }

        Sprite(RawSprite rawSprite)
            : base(rawSprite)
        {
            _rawSprite = rawSprite;
        }

        public void SetAnimation(string animationName)
        {
            if (animationName == null)
                throw new ArgumentNullException("animationName");

            _rawSprite.SetCurrentAnimation(animationName);
            _rawSprite.RestartAnimation();
        }

        public void SetDirection(int direction)
        {
            if (direction < 0 || direction >= _rawSprite.NumDirections)
            {
                string msg = "Illegal direction {0} for sprite '{1}' in animation '{2}'"
                    .F(direction, _rawSprite.AnimationSetId, _rawSprite.CurrentAnimation);
                throw new ArgumentOutOfRangeException("direction", msg);
            }

            _rawSprite.SetCurrentDirection(direction);
        }
    }
}
