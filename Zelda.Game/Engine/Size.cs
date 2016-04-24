using System;

namespace Zelda.Game.Engine
{
    public struct Size
    {
        public int Width;
        public int Height;

        public static Size Zero = new Size(0, 0);

        public bool IsFlat { get { return (Width == 0) || (Height == 0); } }
        public bool IsSquare { get { return Width == Height; } }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static Size operator +(Size size1, Size size2)
        {
            return new Size(size1.Width + size2.Width, size1.Height + size2.Height);
        }

        public static Size operator -(Size size1, Size size2)
        {
            return new Size(size1.Width - size2.Width, size1.Height - size2.Height);
        }

        public static Size operator *(Size size, int factor)
        {
            return new Size(size.Width * factor, size.Height * factor);
        }

        public static Size operator -(Size size, int divisor)
        {
            return new Size(size.Width / divisor, size.Height / divisor);
        }

        public static bool operator ==(Size size1, Size size2)
        {
            return (size1.Width == size2.Width) && (size1.Height == size2.Height);
        }

        public static bool operator !=(Size size1, Size size2)
        {
            return !(size1 == size2);
        }

        public override bool Equals(object obj)
        {
            return obj is Size && Equals((Size)obj);
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

    public static class SizeExts
    {
        public static Size ToSize(this string s)
        {
            var words = s.Split('x');
            if (words == null || words.Length != 2)
                throw new ArgumentException("string {0} does not contain two numbers".F(s));

            return new Size(Convert.ToInt32(words[0]), Convert.ToInt32(words[1]));
        }
    }
}
