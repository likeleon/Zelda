using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptSprite : ScriptDrawable
    {
        readonly Sprite _sprite;

        public Direction4 Direction
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<Direction4>(() =>
                {
                    return _sprite.CurrentDirection;
                });
            }
        }

        public static ScriptSprite Create(string animationSetId)
        {
            return ScriptTools.ExceptionBoundaryHandle<ScriptSprite>(() =>
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
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (animationName == null)
                    throw new ArgumentNullException("animationName");

                _sprite.SetCurrentAnimation(animationName);
                _sprite.RestartAnimation();
            });
        }

        public void SetDirection(Direction4 direction)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
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
