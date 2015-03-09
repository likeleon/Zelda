using System;
using Zelda.Game.Engine;
using Zelda.Game.Movements;
using RawMovement = Zelda.Game.Movements.Movement;
using RawStraightMovement = Zelda.Game.Movements.StraightMovement;
using RawTargetMovement = Zelda.Game.Movements.TargetMovement;

namespace Zelda.Game.Script
{
    public abstract class Movement
    {
        readonly RawMovement _rawMovement;

        public static Movement Create(MovementType type)
        {
            Movement movement = null;
            if (type == MovementType.Target)
            {
                Game game = ScriptContext.MainLoop.Game;
                if (game != null)
                    throw new NotImplementedException("If we are on a map, the default target should be the hero.");
                else
                    movement = new TargetMovement(new RawTargetMovement(new Point(0, 0), 32));
            }
            else
            {
                string enumNames = String.Join(", ", Enum.GetNames(typeof(MovementType)));
                throw new ArgumentOutOfRangeException("type", "should be one of: {0}".F(enumNames));
            }

            movement._rawMovement.IsScriptCallbackEnable = true;
            return movement;
        }

        internal Movement(RawMovement rawMovement)
        {
            _rawMovement = rawMovement;
        }
    }

    public class StraightMovement : Movement
    {
        readonly RawStraightMovement _rawStraightMovement;

        internal StraightMovement(RawStraightMovement rawStraightMovement)
            : base(rawStraightMovement)
        {
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
            _rawTargetMovement.SetMovingSpeed(speed);
        }

        public void SetTarget(Point xy)
        {
            _rawTargetMovement.SetTarget(xy);
        }
    }
}
