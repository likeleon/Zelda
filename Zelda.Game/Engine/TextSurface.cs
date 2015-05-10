using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zelda.Game.Engine
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
        public TextSurface(int x, int y)
            : this(x, y, TextHorizontalAlignment.Left, TextVerticalAlignment.Middle)
        {
        }

        public TextSurface(int x, int y, 
            TextHorizontalAlignment horizontalAlignment, 
            TextVerticalAlignment verticalAlignment)
        {
            _x = x;
            _y = y;
            _horizontalAlignment = horizontalAlignment;
            _verticalAlignment = verticalAlignment;

            _fontId = FontResource.GetDefaultFontId();
            _renderingMode = TextRenderingMode.Solid;
            _textColor = Color.White;
            _fontSize = 11;
            _text = String.Empty;
        }

        int _x;
        public int X
        {
            get { return _x; }
        }

        public void SetX(int x)
        {
            if (x == _x)
                return;

            _x = x;
            Rebuild();
        }

        int _y;
        public int Y
        {
            get { return _y; }
        }

        public void SetY(int y)
        {
            if (y == _y)
                return;

            _y = y;
            Rebuild();
        }

        public void SetPosition(int x, int y)
        {
            if (x == _x && y == _y)
                return;

            _x = x;
            _y = y;
            Rebuild();
        }

        TextHorizontalAlignment _horizontalAlignment;
        public TextHorizontalAlignment HorizontalAlignment
        {
            get { return _horizontalAlignment; }
        }

        public void SetHorizontalAlignment(TextHorizontalAlignment horizontalAlignment)
        {
            if (horizontalAlignment == _horizontalAlignment)
                return;

            _horizontalAlignment = horizontalAlignment;
            
            Rebuild();
        }

        TextVerticalAlignment _verticalAlignment;
        public TextVerticalAlignment VerticalAlignment
        {
            get { return _verticalAlignment; }
        }

        public void SetVerticalAlignment(TextVerticalAlignment verticalAlignment)
        {
            if (verticalAlignment == _verticalAlignment)
                return;

            _verticalAlignment = verticalAlignment;

            Rebuild();
        }

        public void SetAlignment(TextHorizontalAlignment horizontalAlignment,
            TextVerticalAlignment verticalAlignment)
        {
            if (horizontalAlignment == _horizontalAlignment &&
                verticalAlignment == _verticalAlignment)
                return;

            _horizontalAlignment = horizontalAlignment;
            _verticalAlignment = verticalAlignment;
            Rebuild();
        }

        string _fontId;
        public string Font
        {
            get { return _fontId; }
        }
        
        public void SetFont(string fontId)
        {
            if (fontId == _fontId)
                return;

            _fontId = fontId;
            Rebuild();
        }

        TextRenderingMode _renderingMode;
        public TextRenderingMode RenderingMode
        {
            get { return _renderingMode; }
        }

        public void SetRenderingMode(TextRenderingMode renderingMode)
        {
            if (renderingMode == _renderingMode)
                return;

            _renderingMode = renderingMode;
            Rebuild();
        }

        Color _textColor;
        public Color TextColor
        {
            get { return _textColor; }
        }

        public void SetTextColor(Color color)
        {
            if (color == _textColor)
                return;

            _textColor = color;
            Rebuild();
        }

        int _fontSize;
        public int FontSize
        {
            get { return _fontSize; }
        }

        public void SetFontSize(int fontSize)
        {
            if (fontSize == _fontSize)
                return;

            _fontSize = fontSize;
            Rebuild();
        }

        string _text;
        public string Text
        {
            get { return _text; }
        }

        public void SetText(string text)
        {
            if (text == _text)
                return;

            _text = text;
            Rebuild();
        }

        public void AddChar(char c)
        {
            SetText(_text + c);
        }

        public bool IsEmpty
        {
            get { return String.IsNullOrWhiteSpace(_text); }
        }

        Surface _surface;

        public int Width
        {
            get 
            {
                if (_surface == null)
                    return 0;

                return _surface.Width;
            }
        }
        
        public int Height
        {
            get
            {
                if (_surface == null)
                    return 0;

                return _surface.Height;
            }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        Point _textPosition;

        void Rebuild()
        {
            _surface = null;

            if (String.IsNullOrEmpty(_fontId))
                return;

            if (IsEmpty)
                return;

            Debug.CheckAssertion(FontResource.Exists(_fontId), "No such font: '{0}'".F(_fontId));

            if (FontResource.IsBitmapFont(_fontId))
                RebuildBitmap();
            else
                RebuildTtf();

            int xLeft = 0, yTop = 0;

            switch (_horizontalAlignment)
            {
                case TextHorizontalAlignment.Left:
                    xLeft = _x;
                    break;

                case TextHorizontalAlignment.Center:
                    xLeft = _x - _surface.Width / 2;
                    break;

                case TextHorizontalAlignment.Right:
                    xLeft = _x - _surface.Width;
                    break;
            }

            switch (_verticalAlignment)
            {
                case TextVerticalAlignment.Top:
                    yTop = _y;
                    break;

                case TextVerticalAlignment.Middle:
                    yTop = _y - _surface.Height / 2;
                    break;

                case TextVerticalAlignment.Bottom:
                    yTop = _y - _surface.Height;
                    break;
            }

            _textPosition = new Point(xLeft, yTop);
        }

        void RebuildBitmap()
        {
            int numChars = _text.Length;

            // 표면 크기로부터 글자 크기를 결정합니다
            Surface bitmap = FontResource.GetBitmapFont(_fontId);
            Size bitmapSize = bitmap.Size;
            int charWidth = bitmapSize.Width / 128;
            int charHeight = bitmapSize.Height / 16;

            _surface = Surface.Create((charWidth - 1) * numChars + 1, charHeight);

            // 문자들을 그립니다
            Point dstPosition = new Point();
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(_text);
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
            IntPtr internalFont = FontResource.GetOutlineFont(_fontId, _fontSize);
            SDL.SDL_Color internalColor;
            _textColor.GetComponents(out internalColor.r, out internalColor.g, out internalColor.b, out internalColor.a);

            switch (_renderingMode)
            {
                case TextRenderingMode.Solid:
                    internalSurface = SDL_ttf.TTF_RenderUNICODE_Solid(internalFont, _text, internalColor);
                    break;

                case TextRenderingMode.Antialising:
                    internalSurface = SDL_ttf.TTF_RenderUNICODE_Blended(internalFont, _text, internalColor);
                    break;
            }

            Debug.CheckAssertion(internalSurface != IntPtr.Zero,
                "Cannot create the text surface for string '{0}': {1}".F(_text, SDL.SDL_GetError()));

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
    }
}
