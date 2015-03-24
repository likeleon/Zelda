using System;

namespace Zelda.Game.Engine
{
    public struct Size
    {
        readonly int _width;
        public int Width
        {
            get { return _width; }
        }

        readonly int _height;
        public int Height
        {
            get { return _height; }
        }

        public bool IsFlat
        {
            get { return (_width == 0) || (_height == 0); }
        }

        public bool IsSquare
        {
            get { return _width == _height; }
        }

        public Size(int width, int height)
        {
            _width = width;
            _height = height;
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
            return _width ^ _height;
        }

        public override string ToString()
        {
            return "({0},{1})".F(_width, _height);
        }
    }
}
