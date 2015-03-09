using System;

namespace Zelda.Game.Movements
{
    class StraightMovement : Movement
    {
        double _angle;
        public double Angle
        {
            get { return _angle; }
        }

        double _xSpeed;
        public double XSpeed
        {
            get { return _xSpeed; }
        }

        double _ySpeed;
        public double YSpeed
        {
            get { return _ySpeed; }
        }

        public double GetSpeed()
        {
            return Math.Sqrt(_xSpeed * _xSpeed + _ySpeed * _ySpeed);
        }
    }
}
