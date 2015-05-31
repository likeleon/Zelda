using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptSprite : ScriptDrawable
    {
        readonly Sprite _sprite;

        public Direction4 Direction
        {
            get { return _sprite.CurrentDirection; }
        }

        public static ScriptSprite Create(string animationSetId)
        {
            return ScriptToCore.Call(() =>
            {
                Sprite sprite = new Sprite(animationSetId);
                ScriptDrawable.AddDrawable(sprite);
                return new ScriptSprite(sprite);
            });
        }

        ScriptSprite(Sprite sprite)
            : base(sprite)
        {
            _sprite = sprite;
        }

        public void SetAnimation(string animationName)
        {
            ScriptToCore.Call(() =>
            {
                if (animationName == null)
                    throw new ArgumentNullException("animationName");

                _sprite.SetCurrentAnimation(animationName);
                _sprite.RestartAnimation();
            });
        }

        public void SetDirection(Direction4 direction)
        {
            ScriptToCore.Call(() =>
            {
                if (direction < 0 || (int)direction >= _sprite.NumDirections)
                {
                    string msg = "Illegal direction {0} for sprite '{1}' in animation '{2}'"
                        .F(direction, _sprite.AnimationSetId, _sprite.CurrentAnimation);
                    throw new ArgumentOutOfRangeException("direction", msg);
                }

                _sprite.SetCurrentDirection(direction);
            });
        }
    }
}
