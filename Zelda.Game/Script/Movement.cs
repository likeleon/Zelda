﻿using System;
using Zelda.Game.Engine;
using RawMovement = Zelda.Game.Movements.Movement;
using RawStraightMovement = Zelda.Game.Movements.StraightMovement;
using RawTargetMovement = Zelda.Game.Movements.TargetMovement;
using RawGame = Zelda.Game.Game;

namespace Zelda.Game.Script
{
    public abstract class Movement
    {
        public event EventHandler<Point> PositionChanged;
        public event EventHandler MovementFinished;

        readonly RawMovement _rawMovement;

        public static Movement Create(MovementType type)
        {
            return ScriptTools.ExceptionBoundaryHandle<Movement>(() =>
            {
                Movement movement = null;
                if (type == MovementType.Target)
                {
                    RawGame game = ScriptContext.MainLoop.Game;
                    if (game != null)
                        throw new NotImplementedException("If we are on a map, the default target should be the hero.");
                    else
                        movement = new TargetMovement(new RawTargetMovement(new Point(0, 0), 32, false));
                }
                else
                {
                    string enumNames = String.Join(", ", Enum.GetNames(typeof(MovementType)));
                    throw new ArgumentOutOfRangeException("type", "should be one of: {0}".F(enumNames));
                }

                movement._rawMovement.ScriptMovement = movement;
                return movement;
            });
        }

        internal Movement(RawMovement rawMovement)
        {
            _rawMovement = rawMovement;
        }

        public void Start(object objectToMove, Action finishedCallback)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                Stop();

                if (objectToMove is Drawable)
                {
                    Drawable drawable = objectToMove as Drawable;
                    drawable.RawDrawable.StartMovement(_rawMovement);
                }
                else
                {
                    throw new ArgumentException("Point, Entity or Drawable", "objectToMove");
                }
                _rawMovement.FinishedCallback = finishedCallback;
            });
        }

        public void Stop()
        {

        }

        internal void NotifyPositionChanged(Point xy)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
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
            ScriptTools.ExceptionBoundaryHandle(() =>
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

    public class StraightMovement : Movement
    {
        readonly RawStraightMovement _rawStraightMovement;

        internal StraightMovement(RawStraightMovement rawStraightMovement)
            : base(rawStraightMovement)
        {
            _rawStraightMovement = rawStraightMovement;
        }
    }

    public class TargetMovement : StraightMovement
    {
        readonly RawTargetMovement _rawTargetMovement;

        internal TargetMovement(RawTargetMovement rawTargetMovement)
            : base(rawTargetMovement)
        {
            _rawTargetMovement = rawTargetMovement;
        }

        public void SetSpeed(int speed)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _rawTargetMovement.SetMovingSpeed(speed);
            });
        }

        public void SetTarget(Point xy)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _rawTargetMovement.SetTarget(xy);
            });
        }
    }
}
