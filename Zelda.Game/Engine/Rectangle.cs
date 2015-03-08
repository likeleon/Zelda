using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Zelda.Game.Engine
{
    public struct Rectangle
    {
        public int X
        {
            get { return _rect.x; }
        }

        public int Y
        {
            get { return _rect.y; }
        }

        public Point XY
        {
            get { return new Point(X, Y); }
        }

        public int Width
        {
            get { return _rect.w; }
        }

        public int Height
        {
            get { return _rect.h; }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        public Point Center
        {
            get
            {
                return new Point(X + Width / 2, Y + Height / 2);
            }
        }

        public bool IsFlat
        {
            get { return (Width == 0) || (Height == 0); }
        }

        internal SDL.SDL_Rect _rect;

        public Rectangle(int x, int y)
            : this(x, y, 0, 0)
        {
        }

        public Rectangle(Point xy)
            : this(xy.X, xy.Y, 0, 0)
        {
        }

        public Rectangle(Size size)
            : this(0, 0, size.Width, size.Height)
        {
        }

        public Rectangle(int x, int y, int width, int height)
        {
            _rect.x = x;
            _rect.y = y;
            _rect.w = width;
            _rect.h = height;
        }

        public Rectangle(Point xy, Size size)
            : this(xy.X, xy.Y, size.Width, size.Height)
        {
        }

        public bool Contains(int x, int y)
        {
            return (x >= X) && 
                   (x < X + Width) &&
                   (y >= Y) &&
                   (y < Y + Height);
        }

        public bool Overlaps(Rectangle other)
        {
            int x1 = X;
            int x2 = x1 + Width;
            int x3 = other.X;
            int x4 = x3 + other.Width;
            bool overlap_x = (x3 < x2 && x1 < x4);

            int y1 = Y;
            int y2 = y1 + Height;
            int y3 = other.Y;
            int y4 = y3 + other.Height;
            bool overlap_y = (y3 < y2 && y1 < y4);

            return overlap_x && overlap_y && !IsFlat && !other.IsFlat;
        }

        public static Rectangle GetIntersection(Rectangle r1, Rectangle r2)
        {
            Rectangle intersection;
            SDL.SDL_bool intersects = SDL.SDL_IntersectRect(ref r1._rect, ref r2._rect, out intersection._rect);
            if (intersects == SDL.SDL_bool.SDL_TRUE)
                return intersection;
            else
                return new Rectangle(0, 0, 0, 0);
        }

        public static bool operator ==(Rectangle r1, Rectangle r2)
        {
            return (r1.XY == r2.XY) && (r1.Size == r2.Size);
        }
        
        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return (r1.XY != r2.XY) || (r1.Size != r2.Size);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
                return false;

            return (this == (Rectangle)obj);
        }

        public override int GetHashCode()
        {
            return (Height + Width) ^ X + Y;
        }

        public override string ToString()
        {
            return "{0}x{1}".F(XY, Size);
        }
    }
}
