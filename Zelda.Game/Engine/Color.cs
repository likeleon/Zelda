using System;
using System.Xml.Serialization;

namespace Zelda.Game.Engine
{
    public struct Color
    {
        readonly byte _r;
        public byte R
        {
            get { return _r; }
        }

        readonly byte _g;
        public byte G
        {
            get { return _g; }
        }
        
        readonly byte _b;
        public byte B
        {
            get { return _b; }
        }

        readonly byte _a;
        public byte A
        {
            get { return _a; }
        }

        public Color(int r, int g, int b, int a = 255)
        {
            _r = (byte)r;
            _g = (byte)g;
            _b = (byte)b;
            _a = (byte)a;
        }

        public void GetComponents(out byte r, out byte g, out byte b, out byte a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        public static bool operator ==(Color color1, Color color2)
        {
            return ((color1._r == color2._r) &&
                    (color1._g == color2._g) &&
                    (color1._b == color2._b) &&
                    (color1._a == color2._a));
        }

        public static bool operator !=(Color color1, Color color2)
        {
            return ((color1._r != color2._r) ||
                    (color1._g != color2._g) ||
                    (color1._b != color2._b) ||
                    (color1._a != color2._a));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Color))
                return false;

            return (this == (Color)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
     
        public override string ToString()
        {
            return "({0},{1},{2},{3})".F(_r, _g, _b, _a);
        }

        #region 알려진 색들
        public static Color Transparent
        {
            get { return new Color(0, 0, 0, 0); }
        }

        public static Color Black
        {
            get { return new Color(0, 0, 0); }
        }

        public static Color White
        {
            get { return new Color(255, 255, 255); }
        }

        public static Color Red
        {
            get { return new Color(255, 0, 0); }
        }

        public static Color Green
        {
            get { return new Color(0, 255, 0); }
        }

        public static Color Blue
        {
            get { return new Color(0, 0, 255); }
        }

        public static Color Yellow
        {
            get { return new Color(255, 255, 0); }
        }
        
        public static Color Magenta
        {
            get { return new Color(255, 0, 255); }
        }

        public static Color Cyan
        {
            get { return new Color(0, 255, 255); }
        }
        #endregion
    }

    [XmlRoot("Color")]
    public class ColorXmlData
    {
        public int? R { get; set; }
        public int? G { get; set; }
        public int? B { get; set; }
        public int? A { get; set; }
    }
}
