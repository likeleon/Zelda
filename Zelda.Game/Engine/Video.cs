using SDL2;
using System;
using System.ComponentModel;

namespace Zelda.Game.Engine
{
    static class Video
    {
        static Size _modSize;
       
        [Description("렌더링할 모드 표면 크기")]
        public static Size ModSize
        {
            get { return _modSize; }
        }

        public static string WindowTitle
        {
            get { return SDL.SDL_GetWindowTitle(_mainWindow); }
            set { SDL.SDL_SetWindowTitle(_mainWindow, value); }
        }

        static IntPtr _pixelFormat;
        public static IntPtr PixelFormat
        {
            get { return _pixelFormat; }
        }

        static IntPtr _mainWindow;

        static IntPtr _mainRenderer;
        public static IntPtr Renderer
        {
            get { return _mainRenderer; }
        }

        static Size _normalModSize;     // 모드에 설정된 기본 크기
        static Size _minModSize;        // 모드에 설정된 최소 크기
        static Size _maxModSize;        // 모드에 설정된 최대 크기
        static Size _wantedModSize;     // 유저가 원한 크기

        public static void Initialize(Arguments args, string zeldaVersion)
        {
            string modSizeString = args.GetArgumentValue("-mod-size");

            _wantedModSize = new Size(Properties.Settings.Default.DefaultModWidth,
                                      Properties.Settings.Default.DefaultModHeight);

            if (modSizeString != null)
                _wantedModSize = ParseSize(modSizeString);

            CreateWindow(args, zeldaVersion);
        }

        public static void Quit()
        {
            if (_pixelFormat != IntPtr.Zero)
            {
                SDL.SDL_FreeFormat(_pixelFormat);
                _pixelFormat = IntPtr.Zero;
            }
            if (_mainRenderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(_mainRenderer);
                _mainRenderer = IntPtr.Zero;
            }
            if (_mainWindow != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(_mainWindow);
                _mainWindow = IntPtr.Zero;
            }

            _modSize = new Size();
            _wantedModSize = new Size();
        }

        public static Size ParseSize(string sizeString)
        {
            string[] words = sizeString.Split('x');
            Debug.CheckAssertion(words.Length == 2, "sizeString does not contain two numbers");
            return new Size(Convert.ToInt32(words[0]), Convert.ToInt32(words[1]));
        }

        static void CreateWindow(Arguments args, string zeldaVersion)
        {
            _mainWindow = SDL.SDL_CreateWindow(
                "Zelda " + zeldaVersion,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                _wantedModSize.Width,
                _wantedModSize.Height,
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            Debug.CheckAssertion(_mainWindow != IntPtr.Zero, "Cannot create the window: " + SDL.SDL_GetError());

            _mainRenderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC));
            if (_mainRenderer == IntPtr.Zero)
            {
                _mainRenderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));
                if (_mainRenderer == IntPtr.Zero)
                    _mainRenderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));
            }
            Debug.CheckAssertion(_mainRenderer != IntPtr.Zero, "Cannot create the renderer: " + SDL.SDL_GetError());

            SDL.SDL_SetRenderDrawBlendMode(_mainRenderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            SDL.SDL_RendererInfo rendererInfo;
            SDL.SDL_GetRendererInfo(_mainRenderer, out rendererInfo);

            unsafe
            {
                for (uint i = 0; i < rendererInfo.num_texture_formats; ++i)
                {
                    uint format = rendererInfo.texture_formats[i];
                    if (!SDL.SDL_ISPIXELFORMAT_FOURCC(format) && SDL.SDL_ISPIXELFORMAT_ALPHA(format))
                    {
                        _pixelFormat = SDL.SDL_AllocFormat(format);
                        break;
                    }
                }
            }
            Debug.CheckAssertion(_pixelFormat != IntPtr.Zero, "No caompatible pixel format");

            if ((rendererInfo.flags & (uint)SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED) != 0)
                Console.WriteLine("2D acceleration: yes");
            else
                Console.WriteLine("2D acceleration: no");
        }

        static void InitializeVideoModes()
        {
            SDL.SDL_SetWindowFullscreen(_mainWindow, 0);
            SDL.SDL_RenderSetLogicalSize(_mainRenderer, _modSize.Width, _modSize.Height);
            SDL.SDL_ShowCursor(SDL.SDL_ENABLE);
        }

        public static void ShowWindow()
        {
            SDL.SDL_ShowWindow(_mainWindow);
        }

        public static void Render(Surface surface)
        {
            SDL.SDL_SetRenderDrawColor(_mainRenderer, 0, 0, 0, 255);
            SDL.SDL_RenderSetClipRect(_mainRenderer, IntPtr.Zero);
            SDL.SDL_RenderClear(_mainRenderer);
            surface.Render(_mainRenderer);
            SDL.SDL_RenderPresent(_mainRenderer);
        }

        public static void SetModSizeRange(Size normalSize, Size minSize, Size maxSize)
        {
            Debug.CheckAssertion(
                normalSize.Width >= minSize.Width ||
                normalSize.Height >= minSize.Height ||
                normalSize.Width <= maxSize.Width ||
                normalSize.Height <=  maxSize.Height,
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
                _modSize = normalSize;
            }
            else
            {
                _modSize = _wantedModSize;
            }

            InitializeVideoModes();
        }
    }
}
