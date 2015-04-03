using System;

namespace Zelda.Game.Engine
{
    public struct Size
    {
        public int Width;
        public int Height;

        public bool IsFlat
        {
            get { return (Width == 0) || (Height == 0); }
        }

        public bool IsSquare
        {
            get { return Width == Height; }
        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static Size operator +(Size size1, Size size2)
        {
            return new Size(size1.Width + size2.Width, 
                            size1.Height + size2.Height);
        }

        public static Size operator -(Size size1, Size size2)
        {
            return new Size(size1.Width - size2.Width, 
                            size1.Height - size2.Height);
        }

        public static Size operator *(Size size, int factor)
        {
            return new Size(size.Width * factor, 
                            size.Height * factor);
        }

        public static Size operator -(Size size, int divisor)
        {
            return new Size(size.Width / divisor,
                            size.Height / divisor);
        }

        public static bool operator ==(Size size1, Size size2)
        {
            return ((size1.Width == size2.Width) &&
                    (size1.Height == size2.Height));
        }

        public static bool operator !=(Size size1, Size size2)
        {
            return ((size1.Width != size2.Width) ||
                    (size1.Height != size2.Height));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Size))
                return false;

            return (this == (Size)obj);
        }

        public override int GetHashCode()
        {
            return Width ^ Height;
        }

        public override string ToString()
        {
            return "({0},{1})".F(Width, Height);
        }
    }
}
