using SDL2;
using System;

namespace Zelda.Game.LowLevel
{
    public class Video : IDisposable
    {
        // 렌더링할 모드 표면 크기
        public Size ModSize { get; private set; } = Size.Zero;

        public string WindowTitle
        {
            get { return SDL.SDL_GetWindowTitle(_mainWindow); }
            set { SDL.SDL_SetWindowTitle(_mainWindow, value); }
        }

        internal IntPtr PixelFormat { get; private set; }
        internal IntPtr Renderer { get; private set; }

        IntPtr _mainWindow;

        Size _normalModSize;     // 모드에 설정된 기본 크기
        Size _minModSize;        // 모드에 설정된 최소 크기
        Size _maxModSize;        // 모드에 설정된 최대 크기
        Size _wantedModSize;     // 유저가 원한 크기

        internal Video(Arguments args, string zeldaVersion)
        {
            var defaultModSize = new Size(Properties.Settings.Default.DefaultModWidth, Properties.Settings.Default.DefaultModHeight);
            _wantedModSize = args.GetArgumentValue("-mod-size")?.ToSize() ?? defaultModSize;

            CreateWindow(args, zeldaVersion);
        }

        public void Dispose()
        {
            if (PixelFormat != IntPtr.Zero)
                SDL.SDL_FreeFormat(PixelFormat);

            if (Renderer != IntPtr.Zero)
                SDL.SDL_DestroyRenderer(Renderer);

            if (_mainWindow != IntPtr.Zero)
                SDL.SDL_DestroyWindow(_mainWindow);
        }

        void CreateWindow(Arguments args, string zeldaVersion)
        {
            _mainWindow = SDL.SDL_CreateWindow(
                "Zelda " + zeldaVersion,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                _wantedModSize.Width,
                _wantedModSize.Height,
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            Debug.CheckAssertion(_mainWindow != IntPtr.Zero, "Cannot create the window: " + SDL.SDL_GetError());

            Renderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC));
            if (Renderer == IntPtr.Zero)
            {
                Renderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));
                if (Renderer == IntPtr.Zero)
                    Renderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));
            }
            Debug.CheckAssertion(Renderer != IntPtr.Zero, "Cannot create the renderer: " + SDL.SDL_GetError());

            SDL.SDL_SetRenderDrawBlendMode(Renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            SDL.SDL_RendererInfo rendererInfo;
            SDL.SDL_GetRendererInfo(Renderer, out rendererInfo);

            unsafe
            {
                for (uint i = 0; i < rendererInfo.num_texture_formats; ++i)
                {
                    uint format = rendererInfo.texture_formats[i];
                    if (!SDL.SDL_ISPIXELFORMAT_FOURCC(format) && SDL.SDL_ISPIXELFORMAT_ALPHA(format))
                    {
                        PixelFormat = SDL.SDL_AllocFormat(format);
                        break;
                    }
                }
            }
            Debug.CheckAssertion(PixelFormat != IntPtr.Zero, "No caompatible pixel format");

            if ((rendererInfo.flags & (uint)SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED) != 0)
                Console.WriteLine("2D acceleration: yes");
            else
                Console.WriteLine("2D acceleration: no");
        }

        void InitializeVideoModes()
        {
            SDL.SDL_SetWindowFullscreen(_mainWindow, 0);
            SDL.SDL_RenderSetLogicalSize(Renderer, ModSize.Width, ModSize.Height);
            SDL.SDL_ShowCursor(SDL.SDL_ENABLE);
        }

        internal void ShowWindow()
        {
            SDL.SDL_ShowWindow(_mainWindow);
        }

        internal void Render(Surface surface)
        {
            SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);
            SDL.SDL_RenderSetClipRect(Renderer, IntPtr.Zero);
            SDL.SDL_RenderClear(Renderer);
            surface.Render(Renderer);
            SDL.SDL_RenderPresent(Renderer);
        }

        internal void SetModSizeRange(Size normalSize, Size minSize, Size maxSize)
        {
            Debug.CheckAssertion(
                normalSize.Width >= minSize.Width ||
                normalSize.Height >= minSize.Height ||
                normalSize.Width <= maxSize.Width ||
                normalSize.Height <= maxSize.Height,
                "Invalid mod size range");

            _normalModSize = normalSize;
            _minModSize = minSize;
            _maxModSize = maxSize;

            if (_wantedModSize.Width < minSize.Width ||
                _wantedModSize.Height < minSize.Height ||
                _wantedModSize.Width > maxSize.Width ||
                _wantedModSize.Height > maxSize.Height)
            {
                string msg = "Cannot use mod size {0}x{1}".F(_wantedModSize.Width, _wantedModSize.Height);
                msg += ": this mod only supports {0}x{1} to {2}x{3}".F(minSize.Width, minSize.Height, maxSize.Width, maxSize.Height);
                msg += ". Using {0}x{1} instead.".F(normalSize.Width, normalSize.Height);
                Debug.Warning(msg);
                ModSize = normalSize;
            }
            else
            {
                ModSize = _wantedModSize;
            }

            InitializeVideoModes();
        }
    }
}
