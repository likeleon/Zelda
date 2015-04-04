using SDL2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Zelda.Game.Engine
{
    class Surface : Drawable, IDisposable
    {
        public enum ImageDirectory
        {
            Data,       // 데이터 루트
            Sprites,    // 데이터의 sprites 하위 디렉토리 (기본)
            Language    // 데이터의 (현재) 언어 이미지 디렉토리
        }

        class SubSurfaceNode
        {
            public Surface SrcSurface { get; private set; }
            public Rectangle SrcRect { get; private set; }
            public Rectangle DstRect { get; private set; }
            public HashSet<SubSurfaceNode> Subsurfaces { get; private set; }

            public SubSurfaceNode(
                Surface srcSurface,
                Rectangle srcRect,
                Rectangle dstRect,
                HashSet<SubSurfaceNode> subsurfaces)
            {
                SrcSurface = srcSurface;
                SrcRect = srcRect;
                DstRect = dstRect;
                Subsurfaces = subsurfaces;
            }
        }

        class Texture : IDisposable
        {
            readonly IntPtr _internalTexture;
            public IntPtr InternalTexture
            {
                get { return _internalTexture; }
            }

            bool _disposed;

            public Texture(IntPtr internalTexture)
            {
                Debug.CheckAssertion(internalTexture != IntPtr.Zero, "interalTexture should not be IntPtr.Zero");

                _internalTexture = internalTexture;
            }

            ~Texture()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                SDL.SDL_DestroyTexture(_internalTexture);
                _disposed = true;
            }
        }

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

        public Size Size
        {
            get { return new Size(_width, _height); }
        }

        [Description("투명도")]
        byte _internalOpacity = 255;
        public byte Opacity
        {
            set 
            {
                if (_softwareDestination)
                {
                    if (_internalSurface == IntPtr.Zero)
                        CreateSoftwareSurface();

                    ConvertSoftwareSurface();

                    int error = SDL.SDL_SetSurfaceAlphaMod(_internalSurface, value);
                    if (error != 0)
                        Debug.Error(SDL.SDL_GetError());

                    _isRendered = false;
                }
                else
                    _internalOpacity = value;
            }
        }

        [Description("그리기 동작이 RAM과 GPU 어디에서 일어나는지를 의미합니다.")]
        bool _softwareDestination = true;
        public bool SoftwareDestination
        {
            get { return _softwareDestination; }
            set { _softwareDestination = value; }
        }

        readonly HashSet<SubSurfaceNode> _subsurfaces = new HashSet<SubSurfaceNode>();
        IntPtr _internalSurface;
        Texture _internalTexture;
        Color? _internalColor;
        bool _isRendered;
        bool _disposed;

        public static Surface Create(int width, int height)
        {
            return new Surface(width, height);
        }

        public static Surface Create(Size size)
        {
            return new Surface(size.Width, size.Height);
        }

        public static Surface Create(string fileName, ImageDirectory baseDirectory = ImageDirectory.Sprites)
        {
            IntPtr sdlSurface = GetSurfaceFromFile(fileName, baseDirectory);
            if (sdlSurface == IntPtr.Zero)
                return null;

            return new Surface(sdlSurface);
        }

        private static IntPtr GetSurfaceFromFile(string fileName, ImageDirectory baseDirectory)
        {
            string prefix = String.Empty;
            bool languageSpecific = false;

            if (baseDirectory == ImageDirectory.Sprites)
            {
                prefix = "sprites/";
            }
            else if (baseDirectory == ImageDirectory.Language)
            {
                languageSpecific = true;
                prefix = "images/";
            }
            string prefixedFileName = prefix + fileName;

            if (!ModFiles.DataFileExists(prefixedFileName, languageSpecific))
                return IntPtr.Zero;

            IntPtr softwareSurface;
            using (MemoryStream stream = ModFiles.DataFileRead(prefixedFileName, languageSpecific))
            {
                byte[] buffer = stream.ToArray();
                IntPtr rw = SDL.SDL_RWFromMem(buffer, buffer.Length);
                softwareSurface = SDL_image.IMG_Load_RW(rw, 0);
                // TODO: 네이티브의 SDL_RWclose(rw)에 해당하는 구현 방법을 찾지 못했다. 메모리 릭 확인할 것.
            }

            Debug.CheckAssertion(softwareSurface != IntPtr.Zero, "Cannot load image '{0}'".F(prefixedFileName));

            return softwareSurface;
        }

        Surface(int width, int height)
        {
            Debug.CheckAssertion(width > 0 && height > 0, "Attempt to create a surface with an empty size");

            _width = width;
            _height = height;
        }

        Surface(IntPtr internalSurface)
        {
            _internalSurface = internalSurface;
            _width = _internalSurface.ToSDLSurface().w;
            _height = _internalSurface.ToSDLSurface().h;
        }

        ~Surface()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_internalSurface != IntPtr.Zero)
                SDL.SDL_FreeSurface(_internalSurface);

            _disposed = true;
        }

        public void Render(IntPtr renderer)
        {
            Rectangle size = new Rectangle(Size);
            Render(renderer, size, size, size, 255, _subsurfaces);
        }

        void Render(
            IntPtr renderer, 
            Rectangle srcRect, 
            Rectangle dstRect, 
            Rectangle clipRect, 
            byte opacity, 
            HashSet<SubSurfaceNode> subsurfaces)
        {
            if (_internalSurface != IntPtr.Zero)
            {
                if (_internalTexture == null)
                    CreateTextureFromSurface();

                // 소프트웨어 표면에 변경이 있다면 하드웨어 텍스쳐를 갱신합니다
                else if (SoftwareDestination && !_isRendered)
                {
                    ConvertSoftwareSurface();
                    SDL.SDL_UpdateTexture(
                        _internalTexture.InternalTexture,
                        IntPtr.Zero,
                        _internalSurface.ToSDLSurface().pixels,
                        _internalSurface.ToSDLSurface().pitch);
                    SDL.SDL_GetSurfaceAlphaMod(_internalSurface, out _internalOpacity);
                }
            }

            byte currentOpacity = Math.Min(_internalOpacity, opacity);

            // 배경색을 그립니다
            if (_internalColor != null)
            {
                byte r, g, b, a;
                _internalColor.Value.GetComponents(out r, out g, out b, out a);
                SDL.SDL_SetRenderDrawColor(renderer, r, g, b, Math.Min(a, currentOpacity));
                SDL.SDL_RenderFillRect(renderer, ref clipRect._rect);
            }

            // 내부 텍스쳐를 그립니다
            if (_internalTexture != null)
            {
                SDL.SDL_SetTextureAlphaMod(_internalTexture.InternalTexture, currentOpacity);

                SDL.SDL_RenderCopy(
                    renderer,
                    _internalTexture.InternalTexture,
                    ref srcRect._rect,
                    ref dstRect._rect);
            }

            // 현재 표면은 모두 그려졌고 이어서 하위 표면들의 텍스쳐들을 그립니다
            foreach (SubSurfaceNode subsurface in _subsurfaces)
            {
                // 스크린 상의 절대 좌표 계산
                Rectangle subsurfaceDstRect = new Rectangle(
                    x: dstRect.X + subsurface.DstRect.X - srcRect.X,
                    y: dstRect.Y + subsurface.DstRect.Y - srcRect.Y,
                    width: subsurface.SrcRect.Width,
                    height: subsurface.SrcRect.Height);

                // 딸린 표면의 타겟 영역과 이 표면의 클리핑 영역을 가지고 클리핑 영역 계산
                Rectangle superimposedClipRect;
                if (SDL.SDL_IntersectRect(
                    ref subsurfaceDstRect._rect, 
                    ref clipRect._rect, 
                    out superimposedClipRect._rect) == SDL.SDL_bool.SDL_TRUE)
                {
                    subsurface.SrcSurface.Render(
                        renderer,
                        subsurface.SrcRect,
                        subsurfaceDstRect,
                        superimposedClipRect,
                        currentOpacity,
                        subsurface.Subsurfaces);
                }
            }

            _isRendered = true;
        }

        public void Clear()
        {
            ClearSubsurfaces();

            _internalColor = null;

            if (_internalSurface != null)
            {
                if (_softwareDestination)
                    SDL.SDL_FillRect(_internalSurface, IntPtr.Zero, GetColorValue(Color.Transparent));
                else
                    _internalSurface = IntPtr.Zero;
            }
        }

        public void Clear(Rectangle where)
        {
            Debug.CheckAssertion(_softwareDestination, 
                "Partial surface clear is only supported with software surfaces");

            if (_internalSurface == null)
                return;

            SDL.SDL_FillRect(_internalSurface, ref where._rect, GetColorValue(Color.Transparent));
            _isRendered = false;
        }

        void ClearSubsurfaces()
        {
            _subsurfaces.Clear();
        }

        public override void RawDraw(Surface dstSurface, Point dstPosition)
        {
            Rectangle region = new Rectangle(0, 0, _width, _height);
            RawDrawRegion(region, dstSurface, dstPosition);
        }

        public override void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            if (dstSurface.SoftwareDestination)
            {
                if (dstSurface._internalSurface == IntPtr.Zero)
                    dstSurface.CreateSoftwareSurface();

                if (_subsurfaces.Count > 0)
                {
                    if (_internalSurface == IntPtr.Zero)
                        CreateSoftwareSurface();

                    HashSet<SubSurfaceNode> subsurfaces = new HashSet<SubSurfaceNode>(_subsurfaces);
                    _subsurfaces.Clear();

                    foreach (SubSurfaceNode subsurface in subsurfaces)
                    {
                        subsurface.SrcSurface.RawDrawRegion(
                            subsurface.SrcRect,
                            this,
                            subsurface.DstRect.XY);
                    }
                    ClearSubsurfaces();
                }

                if (_internalSurface != IntPtr.Zero)
                {
                    Rectangle dstRect = new Rectangle(dstPosition);
                    SDL.SDL_BlitSurface(
                        _internalSurface,
                        ref region._rect,
                        dstSurface._internalSurface,
                        ref dstRect._rect);
                }
                else if (_internalColor != null)
                {
                    if (_internalColor.Value.A == 255)
                    {
                        // 불투명. 대상 픽셀을 직접 수정할 수 있습니다
                        Rectangle dstRect = new Rectangle(dstPosition, region.Size);
                        SDL.SDL_FillRect(
                            dstSurface._internalSurface,
                            ref dstRect._rect,
                            GetColorValue(_internalColor.Value));
                    }
                    else
                    {
                        // 반투명. 알파 블렌딩이 필요합니다
                        CreateSoftwareSurface();
                        SDL.SDL_FillRect(
                            _internalSurface,
                            IntPtr.Zero,
                            GetColorValue(_internalColor.Value));
                        Rectangle dstRect = new Rectangle(dstPosition);
                        SDL.SDL_BlitSurface(
                            _internalSurface,
                            ref region._rect,
                            dstSurface._internalSurface,
                            ref dstRect._rect);
                    }
                }
            }
            else
            {
                // 목표가 GPU 표면인 경우(텍스쳐).
                // 명시적으로 그리기를 하지 않고 추후에 GPU에 의해 렌더링될 수 있도록
                // 트리에 명령만 추가합니다.
                dstSurface.AddSubSurface(this, region, dstPosition);
            }

            dstSurface._isRendered = false;
        }

        void AddSubSurface(Surface srcSurface, Rectangle region, Point dstPosistion)
        {
            SubSurfaceNode node = new SubSurfaceNode(
                srcSurface, 
                region, 
                new Rectangle(dstPosistion), 
                srcSurface._subsurfaces);

            // 현재 dst_surface가 이미 렌더링된 상태라면 subsurface 리스트를 비운다
            if (_isRendered)
                ClearSubsurfaces();

            _subsurfaces.Add(node);
        }

        // 내부 표면을 소프트웨어 모드로 생성합니다
        void CreateSoftwareSurface()
        {
            Debug.CheckAssertion(_internalSurface == IntPtr.Zero, "Software surface already exists");

            SDL.SDL_PixelFormat format = Video.PixelFormat.ToSDLPixelFormat();
            _internalSurface = SDL.SDL_CreateRGBSurface(
                0,
                _width,
                _height,
                32,
                format.Rmask,
                format.Gmask,
                format.Bmask,
                format.Amask);
            SDL.SDL_SetSurfaceBlendMode(_internalSurface, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            _isRendered = false;

            Debug.CheckAssertion(_internalSurface != IntPtr.Zero, "Failed to create software surface");
        }

        // 소프트웨어 표면을 32-bit 알파 채널의 픽셀 포맷으로 변경합니다
        void ConvertSoftwareSurface()
        {
            Debug.CheckAssertion(_internalSurface != IntPtr.Zero, "Missing software surface to convert");

            SDL.SDL_PixelFormat videoPixelFormat = Video.PixelFormat.ToSDLPixelFormat();
            SDL.SDL_PixelFormat surfacePixelFormat = _internalSurface.ToSDLSurface().format.ToSDLPixelFormat();
            if (surfacePixelFormat.format != videoPixelFormat.format)
            {
                byte opacity;
                SDL.SDL_GetSurfaceAlphaMod(_internalSurface, out opacity);
                IntPtr convertedSurface = SDL.SDL_ConvertSurface(
                    _internalSurface, 
                    Video.PixelFormat, 
                    0);
                Debug.CheckAssertion(convertedSurface != IntPtr.Zero, "Failed to convert software surface");

                _internalSurface = convertedSurface;
                SDL.SDL_SetSurfaceAlphaMod(_internalSurface, opacity);  // alpha값 복구 
            }
        }

        // 색상 값을 현재 비디오 픽셀 포맷에 맞는 32-bit 값으로 변환합니다
        uint GetColorValue(Color color)
        {
            return SDL.SDL_MapRGBA(Video.PixelFormat, color.R, color.G, color.B, color.A);
        }

        // 소프트웨어 표면으로부터 하드웨어 텍스쳐를 생성합니다
        void CreateTextureFromSurface()
        {
            IntPtr mainRenderer = Video.Renderer;
            if (mainRenderer != IntPtr.Zero)
            {
                Debug.CheckAssertion(_internalSurface != IntPtr.Zero, "Missing software surface to create texture from");

                // 성능의 이유로 SDL_UpdateTexture가 픽셀 포맷을 인자로 받아들이지 않기 때문에
                // 소프트웨어 표면이 텍스쳐와 같은 포맷이어야 합니다
                ConvertSoftwareSurface();

                IntPtr sdlTexture = SDL.SDL_CreateTexture(
                    mainRenderer,
                    Video.PixelFormat.ToSDLPixelFormat().format,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC,
                    _internalSurface.ToSDLSurface().w,
                    _internalSurface.ToSDLSurface().h);
                _internalTexture = new Texture(sdlTexture);
                SDL.SDL_SetTextureBlendMode(_internalTexture.InternalTexture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                // 소프트웨어 표면의 픽셀들을 GPU 텍스쳐로 복사합니다
                SDL.SDL_UpdateTexture(_internalTexture.InternalTexture,
                    IntPtr.Zero,
                    _internalSurface.ToSDLSurface().pixels,
                    _internalSurface.ToSDLSurface().pitch);
                SDL.SDL_GetSurfaceAlphaMod(_internalSurface, out _internalOpacity);
            }
        }

        public void FillWithColor(Color color, Rectangle? where = null)
        {
            Rectangle fillwhere = where ?? new Rectangle(0, 0, _width, _height);
            Surface coloredSurface = Surface.Create(fillwhere.Size);
            coloredSurface.SoftwareDestination = false;
            coloredSurface._internalColor = color;
            coloredSurface.RawDrawRegion(new Rectangle(coloredSurface.Size), this, fillwhere.XY);
        }
    }
}
