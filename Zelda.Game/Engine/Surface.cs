using SDL2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Zelda.Game.Engine
{
    class Surface : Drawable
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
                _internalOpacity = value;
            }
        }

        //[Description("그리기 동작이 RAM과 GPU 어디에서 일어나는지를 의미합니다.")]
        //bool _softwareDestination = true;
        //public bool SoftwareDestination
        //{
        //    get { return _softwareDestination; }
        //    set { _softwareDestination = value; }
        //}

        readonly HashSet<SubSurfaceNode> _subsurfaces = new HashSet<SubSurfaceNode>();
        IntPtr _internalSurface;
        bool _isRendered;

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
                prefix = "Sprites/";
            }
            else if (baseDirectory == ImageDirectory.Language)
            {
                languageSpecific = true;
                prefix = "Images/";
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

            if (softwareSurface == IntPtr.Zero)
                throw new Exception("Cannot load image '" + prefixedFileName + "'");

            return softwareSurface;
        }

        Surface(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("", "Attempt to create a surface with an empty size");

            _width = width;
            _height = height;
        }

        Surface(IntPtr internalSurface)
        {
            _internalSurface = internalSurface;
            _width = _internalSurface.GetStruct().w;
            _height = _internalSurface.GetStruct().h;
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
            }

            byte currentOpacity = Math.Min(_internalOpacity, opacity);

            foreach (SubSurfaceNode subsurface in _subsurfaces)
            {
                // 딸린 표면들은 이 표면에 그려져야 함

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
                        subsurface.DstRect,
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
            dstSurface.AddSubSurface(this, region, dstPosition);
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
    }
}
