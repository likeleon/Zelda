using SDL2;
using System;
using System.Text;

namespace Zelda.Game.LowLevel
{
    public enum TextHorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum TextVerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public enum TextRenderingMode
    {
        Solid,
        Antialising
    }

    class TextSurface : Drawable
    {
        public static readonly int DefaultFontSize = 11;

        Surface _surface;
        Point _textPosition;

        public int X { get; private set; }
        public int Y { get; private set; }
        public TextHorizontalAlignment HorizontalAlignment { get; private set; }
        public TextVerticalAlignment VerticalAlignment { get; private set; }
        public TextRenderingMode RenderingMode { get; private set; }
        public string Font { get; private set; }
        public Color TextColor { get; private set; }
        public int FontSize { get; private set; }
        public string Text { get; private set; }
        public bool IsEmpty { get { return String.IsNullOrWhiteSpace(Text); } }

        public int Width { get { return _surface != null ? _surface.Width : 0; } }
        public int Height { get { return _surface != null ? _surface.Height : 0; } }
        public Size Size { get { return new Size(Width, Height); } }

        public override Surface TransitionSurface { get { return _surface; } }
   
        public TextSurface(int x, int y)
            : this(x, y, TextHorizontalAlignment.Left, TextVerticalAlignment.Middle)
        {
        }

        public TextSurface(int x, int y, 
            TextHorizontalAlignment horizontalAlignment, 
            TextVerticalAlignment verticalAlignment)
        {
            X = x;
            Y = y;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;

            Font = Core.FontResource.GetDefaultFontId();
            RenderingMode = TextRenderingMode.Solid;
            TextColor = Color.White;
            FontSize = 11;
            Text = String.Empty;
        }

        public void SetX(int x)
        {
            if (x == X)
                return;

            X = x;
            Rebuild();
        }

        public void SetY(int y)
        {
            if (y == Y)
                return;

            Y = y;
            Rebuild();
        }

        public void SetPosition(int x, int y)
        {
            if (x == X && y == Y)
                return;

            X = x;
            Y = y;
            Rebuild();
        }

        public void SetHorizontalAlignment(TextHorizontalAlignment horizontalAlignment)
        {
            if (horizontalAlignment == HorizontalAlignment)
                return;

            HorizontalAlignment = horizontalAlignment;
            
            Rebuild();
        }

        public void SetVerticalAlignment(TextVerticalAlignment verticalAlignment)
        {
            if (verticalAlignment == VerticalAlignment)
                return;

            VerticalAlignment = verticalAlignment;

            Rebuild();
        }

        public void SetAlignment(TextHorizontalAlignment horizontalAlignment,
            TextVerticalAlignment verticalAlignment)
        {
            if (horizontalAlignment == HorizontalAlignment &&
                verticalAlignment == VerticalAlignment)
                return;

            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            Rebuild();
        }
        
        public void SetFont(string fontId)
        {
            if (fontId == Font)
                return;

            Font = fontId;
            Rebuild();
        }

        public void SetRenderingMode(TextRenderingMode renderingMode)
        {
            if (renderingMode == RenderingMode)
                return;

            RenderingMode = renderingMode;
            Rebuild();
        }

        public void SetTextColor(Color color)
        {
            if (color == TextColor)
                return;

            TextColor = color;
            Rebuild();
        }

        public void SetFontSize(int fontSize)
        {
            if (fontSize == FontSize)
                return;

            FontSize = fontSize;
            Rebuild();
        }

        public void SetText(string text)
        {
            if (text == Text)
                return;

            Text = text;
            Rebuild();
        }

        public void AddChar(char c)
        {
            SetText(Text + c);
        }

        void Rebuild()
        {
            _surface = null;

            if (String.IsNullOrEmpty(Font))
                return;

            if (IsEmpty)
                return;

            Debug.CheckAssertion(Core.FontResource.Exists(Font), "No such font: '{0}'".F(Font));

            if (Core.FontResource.IsBitmapFont(Font))
                RebuildBitmap();
            else
                RebuildTtf();

            int xLeft = 0, yTop = 0;

            switch (HorizontalAlignment)
            {
                case TextHorizontalAlignment.Left:
                    xLeft = X;
                    break;

                case TextHorizontalAlignment.Center:
                    xLeft = X - _surface.Width / 2;
                    break;

                case TextHorizontalAlignment.Right:
                    xLeft = X - _surface.Width;
                    break;
            }

            switch (VerticalAlignment)
            {
                case TextVerticalAlignment.Top:
                    yTop = Y;
                    break;

                case TextVerticalAlignment.Middle:
                    yTop = Y - _surface.Height / 2;
                    break;

                case TextVerticalAlignment.Bottom:
                    yTop = Y - _surface.Height;
                    break;
            }

            _textPosition = new Point(xLeft, yTop);
        }

        void RebuildBitmap()
        {
            int numChars = Text.Length;

            // 표면 크기로부터 글자 크기를 결정합니다
            Surface bitmap = Core.FontResource.GetBitmapFont(Font);
            Size bitmapSize = bitmap.Size;
            int charWidth = bitmapSize.Width / 128;
            int charHeight = bitmapSize.Height / 16;

            _surface = Surface.Create((charWidth - 1) * numChars + 1, charHeight);

            // 문자들을 그립니다
            Point dstPosition = new Point();
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(Text);
            for (int i = 0; i < utf8Bytes.Length; ++i)
            {
                char firstByte = (char)utf8Bytes[i];
                Rectangle srcPosition = new Rectangle(0, 0, charWidth, charHeight);
                if ((firstByte & 0xE0) != 0xC0)
                {
                    // 1바이트 문자
                    srcPosition.XY = new Point(firstByte * charWidth, 0);
                }
                else
                {
                    // 2바이트 문자
                    ++i;
                    char secondByte = (char)utf8Bytes[i];
                    int codePoint = ((firstByte & 0x1F) << 6) | (secondByte & 0x3F);
                    srcPosition.XY = new Point((codePoint % 128) * charWidth,
                        (codePoint / 128) * charHeight);
                }
                bitmap.DrawRegion(srcPosition, _surface, dstPosition);
                dstPosition.X += charWidth - 1;
            }
        }

        void RebuildTtf()
        {
            IntPtr internalSurface = IntPtr.Zero;
            IntPtr internalFont = Core.FontResource.GetOutlineFont(Font, FontSize);
            SDL.SDL_Color internalColor;
            TextColor.GetComponents(out internalColor.r, out internalColor.g, out internalColor.b, out internalColor.a);

            switch (RenderingMode)
            {
                case TextRenderingMode.Solid:
                    internalSurface = SDL_ttf.TTF_RenderUNICODE_Solid(internalFont, Text, internalColor);
                    break;

                case TextRenderingMode.Antialising:
                    internalSurface = SDL_ttf.TTF_RenderUNICODE_Blended(internalFont, Text, internalColor);
                    break;
            }

            Debug.CheckAssertion(internalSurface != IntPtr.Zero,
                "Cannot create the text surface for string '{0}': {1}".F(Text, SDL.SDL_GetError()));

            _surface = new Surface(internalSurface);
        }

        public override void RawDraw(Surface dstSurface, Point dstPosition)
        {
            if (_surface != null)
                _surface.RawDraw(dstSurface, dstPosition + _textPosition);
        }

        public override void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            if (_surface != null)
                _surface.RawDrawRegion(region, dstSurface, dstPosition + _textPosition);
        }

        public override void DrawTransition(Transition transition)
        {
            Transition.Draw(_surface);
        }
    }
}
