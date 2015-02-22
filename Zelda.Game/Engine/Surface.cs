using SDL2;
using System;
using System.Collections.Generic;

namespace Zelda.Game.Engine
{
    class Surface : Drawable
    {
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

        readonly HashSet<SubSurfaceNode> _subsurfaces = new HashSet<SubSurfaceNode>();
        bool _isRendered;
        byte _internalOpacity = 255;

        public static Surface Create(int width, int height)
        {
            return new Surface(width, height);
        }

        public static Surface Create(Size size)
        {
            return new Surface(size.Width, size.Height);
        }

        Surface(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException("", "Attempt to create a surface with an empty size");

            _width = width;
            _height = height;
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
