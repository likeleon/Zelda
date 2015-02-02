using System;

namespace Zelda.Game.Engine
{
    public struct Point
    {
        private readonly int _x;
        public int X
        {
            get { return _x; }
        }

        private readonly int _y;
        public int Y
        {
            get { return _y; }
        }

        public Point(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public static Point operator +(Point point1, Point point2)
        {
            return new Point(point1._x + point2._x, 
                             point1._y + point2._y);
        }

        public static Point operator -(Point point1, Point point2)
        {
            return new Point(point1._x - point2._x,
                             point1._y - point2._y);
        }

        public static Point operator *(Point point1, int factor)
        {
            return new Point(point1._x * factor,
                             point1._y * factor);
        }

        public static Point operator /(Point point1, int divisor)
        {
            return new Point(point1._x / divisor,
                             point1._y / divisor);
        }

        public static bool operator ==(Point point1, Point point2)
        {
            return ((point1.X == point2.X) &&
                    (point1.Y == point2.Y));
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return ((point1.X != point2.X) ||
                    (point1.Y != point2.Y));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            return (this == (Point)obj);
        }

        public override int GetHashCode()
        {
            return _x ^ _y;
        }

        public override string ToString()
        {
            return String.Format("({0},{1}", _x, _y);
        }
    }
}
