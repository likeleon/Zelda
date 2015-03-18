﻿using System;

namespace Zelda.Game.Engine
{
    static class Geometry
    {
        public static double PI = 3.14159265358979323846;
        public static double TwoPI = 2.0 * PI;
        public static double PIOver2 = PI / 2.0;

        public static double GetAngle(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            // x와 y가 모두 0인 경우의 atan2 결과는 정의되어 있지 않습니다
            if (dx == 0 && dy == 0)
                return PIOver2;

            double angle = Math.Atan2(-dy, dx);

            // Normaize
            if (angle < 0)
                angle += TwoPI;

            return angle;
        }

        public static double GetAngle(Point point1, Point point2)
        {
            return GetAngle(point1.X, point1.Y, point2.X, point2.Y);
        }

        public static double GetDistance(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            return Math.Sqrt((double)(dx * dx + dy * dy));
        }

        public static double GetDistance(Point point1, Point point2)
        {
            return GetDistance(point1.X, point1.Y, point2.X, point2.Y);
        }
    }
}