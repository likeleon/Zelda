using System;
using Zelda.Game.Lowlevel;
using Zelda.Game.Movements;

namespace Zelda.Game.Script
{
    public abstract class ScriptMovement
    {
        public event EventHandler<Point> PositionChanged;
        public event EventHandler MovementFinished;

        readonly Movement _movement;

        public static ScriptMovement Create(MovementType type)
        {
            return ScriptToCore.Call(() =>
            {
                ScriptMovement movement = null;
                if (type == MovementType.Target)
                {
                    Game game = ScriptContext.MainLoop.Game;
                    if (game != null)
                        throw new NotImplementedException("If we are on a map, the default target should be the hero.");
                    else
                        movement = new ScriptTargetMovement(new TargetMovement(new Point(0, 0), 32, false));
                }
                else
                {
                    string enumNames = String.Join(", ", Enum.GetNames(typeof(MovementType)));
                    throw new ArgumentOutOfRangeException("type", "should be one of: {0}".F(enumNames));
                }

                movement._movement.ScriptMovement = movement;
                return movement;
            });
        }

        internal ScriptMovement(Movement movement)
        {
            _movement = movement;
        }

        public void Start(object objectToMove, Action finishedCallback)
        {
            ScriptToCore.Call(() =>
            {
                Stop();

                if (objectToMove is ScriptDrawable)
                {
                    ScriptDrawable drawable = objectToMove as ScriptDrawable;
                    drawable.Drawable.StartMovement(_movement);
                }
                else
                {
                    throw new ArgumentException("Point, Entity or Drawable", "objectToMove");
                }
                _movement.FinishedCallback = finishedCallback;
            });
        }

        public void Stop()
        {
        }

        internal void NotifyPositionChanged(Point xy)
        {
            ScriptToCore.Call(() =>
            {
                OnPositionChanged(xy);
                if (PositionChanged != null)
                    PositionChanged(this, xy);
            });
        }

        public virtual void OnPositionChanged(Point xy)
        {
        }

        internal void NotifyMovementFinished()
        {
            ScriptToCore.Call(() =>
            {
                OnMovementFinished();
                if (MovementFinished != null)
                    MovementFinished(this, EventArgs.Empty);
            });
        }

        public virtual void OnMovementFinished()
        {
        }
    }

    public class ScriptStraightMovement : ScriptMovement
    {
        readonly StraightMovement _straightMovement;

        internal ScriptStraightMovement(StraightMovement straightMovement)
            : base(straightMovement)
        {
            _straightMovement = straightMovement;
        }
    }

    public class ScriptTargetMovement : ScriptStraightMovement
    {
        readonly TargetMovement _targetMovement;

        internal ScriptTargetMovement(TargetMovement targetMovement)
            : base(targetMovement)
        {
            _targetMovement = targetMovement;
        }

        public void SetSpeed(int speed)
        {
            ScriptToCore.Call(() => _targetMovement.SetMovingSpeed(speed));
        }

        public void SetTarget(Point xy)
        {
            ScriptToCore.Call(() => _targetMovement.SetTarget(xy));
        }
    }
}
