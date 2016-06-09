namespace Zelda.Game.LowLevel
{
    public struct Color
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public Color(int r, int g, int b, int a = 255)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)a;
        }

        public static bool operator ==(Color color1, Color color2)
        {
            return (color1.R == color2.R) && (color1.G == color2.G) && (color1.B == color2.B) && (color1.A == color2.A);
        }

        public static bool operator !=(Color color1, Color color2)
        {
            return (color1.R != color2.R) || (color1.G != color2.G) || (color1.B != color2.B) || (color1.A != color2.A);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Color))
                return false;

            return (this == (Color)obj);
        }

        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => "({0},{1},{2},{3})".F(R, G, B, A);

        public static Color Transparent = new Color(0, 0, 0, 0);
        public static Color Black = new Color(0, 0, 0);
        public static Color White = new Color(255, 255, 255);
        public static Color Red = new Color(255, 0, 0);
        public static Color Green = new Color(0, 255, 0);
        public static Color Blue = new Color(0, 0, 255);
        public static Color Yellow = new Color(255, 255, 0);
        public static Color Magenta = new Color(255, 0, 255);
        public static Color Cyan = new Color(0, 255, 255);
    }
}
