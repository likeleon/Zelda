﻿using SDL2;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace Zelda.Game.Engine
{
    public static class Video
    {
        private static Size _gameSize;
        public static Size GameSize
        {
            get { return _gameSize; }
        }

        private static IntPtr _mainWindow;
        private static IntPtr _mainRenderer;
        private static IntPtr _pixelFormat;
        private static Size _wantedGameSize;

        public static void Initialize(Arguments args)
        {
            string gameSizeString = args.GetValue("-game-size", null);

            _wantedGameSize = Properties.Settings.Default.DefaultGameSize;

            if (gameSizeString != null)
                _wantedGameSize = ParseSize(gameSizeString);

            CreateWindow(args);
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

            _gameSize = new Size();
            _wantedGameSize = new Size();
        }

        public static void DetermineGameSize()
        {
            _gameSize = _wantedGameSize;

            // 게임 크기가 결정되었기 때문에 초기화를 완료한다
            InitializeVideoModes();
        }

        private static Size ParseSize(string sizeString)
        {
            string[] words = sizeString.Split('x');
            if (words.Length < 2)
                throw new InvalidOperationException("sizeString does not contain two numbers");
            if (words.Length > 2)
                throw new InvalidOperationException("sizeString contains too many delimiters");

            return new Size(Convert.ToInt32(words[0]), Convert.ToInt32(words[1]));
        }

        private static void CreateWindow(Arguments args)
        {
            _mainWindow = SDL.SDL_CreateWindow(
                "Zelda " + GetVersion(),
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                _wantedGameSize.Width,
                _wantedGameSize.Height,
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (_mainWindow == IntPtr.Zero)
                throw new Exception("Cannot create the window: " + SDL.SDL_GetError());

            _mainRenderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC));
            if (_mainRenderer == IntPtr.Zero)
            {
                _mainRenderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));
                if (_mainRenderer == IntPtr.Zero)
                    _mainRenderer = SDL.SDL_CreateRenderer(_mainWindow, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));
            }
            if (_mainRenderer == IntPtr.Zero)
                throw new Exception("Cannot create the renderer: " + SDL.SDL_GetError());

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
            if (_pixelFormat == IntPtr.Zero)
                throw new Exception("No caompatible pixel format");

            if ((rendererInfo.flags & (uint)SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED) != 0)
                Console.WriteLine("2D acceleration: yes");
            else
                Console.WriteLine("2D acceleration: no");
        }

        private static void InitializeVideoModes()
        {
            SDL.SDL_SetWindowFullscreen(_mainWindow, 0);
            SDL.SDL_RenderSetLogicalSize(_mainRenderer, _gameSize.Width, _gameSize.Height);
            SDL.SDL_ShowCursor(SDL.SDL_ENABLE);
        }

        public static void ShowWindow()
        {
            SDL.SDL_ShowWindow(_mainWindow);
        }

        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}
