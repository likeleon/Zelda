using System;
using RawMovement = Zelda.Game.Movements.Movement;

namespace Zelda.Game.Script
{
    public class Movement
    {
        readonly RawMovement _rawMovement;

        public static Movement Create(MovementType type)
        {
            RawMovement movement = null;
            if (type == MovementType.Target)
            {
                Game game = ScriptContext.MainLoop.Game;
                if (game != null)
                    throw new NotImplementedException("If we are on a map, the default target should be the hero.");
                else
                    movement = new TargetMovement(null, 0, 0, 32, false);
            }
            else
            {
                string enumNames = String.Join(", ", Enum.GetNames(typeof(MovementType)));
                throw new ArgumentOutOfRangeException("type", "should be one of: {0}".F(enumNames));
            }

            movement.IsScriptCallbackEnable = true;
            return new Movement(movement);
        }

        Movement(RawMovement rawMovement)
        {
            _rawMovement = rawMovement;
        }
    }
}
