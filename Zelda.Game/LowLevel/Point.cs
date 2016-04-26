using System;

namespace Zelda.Game.LowLevel
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, 
                             point1.Y + point2.Y);
        }

        public static Point operator -(Point point1, Point point2)
        {
            return new Point(point1.X - point2.X,
                             point1.Y - point2.Y);
        }

        public static Point operator *(Point point1, int factor)
        {
            return new Point(point1.X * factor,
                             point1.Y * factor);
        }

        public static Point operator /(Point point1, int divisor)
        {
            return new Point(point1.X / divisor,
                             point1.Y / divisor);
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
            return X ^ Y;
        }

        public override string ToString()
        {
            return "({0},{1})".F(X, Y);
        }
    }
}
