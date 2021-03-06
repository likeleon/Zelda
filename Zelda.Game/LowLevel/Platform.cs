﻿using SDL2;
using System;

namespace Zelda.Game.LowLevel
{
    class Platform : IDisposable
    {
        public string Os => SDL.SDL_GetPlatform();
        public Video Video { get; }
        public Audio Audio { get; }
        public Input Input { get; }
        public FontResource FontResource { get; }
        public SpriteSystem SpriteSystem { get; }

        readonly int _initialTime;       // 초기화 시점의 실제 시각, 밀리초

        public Platform(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = GetRealTime();

            if (!args.HasArgument("-no-audio"))
                Audio = new Audio(args);
            Input = new Input();
            Video = new Video(args, ZeldaVersion.Version.ToString());
            FontResource = new FontResource();
            SpriteSystem = new SpriteSystem();
        }

        public void Dispose()
        {
            Input?.Dispose();
            Audio?.Dispose();
            SpriteSystem?.Dispose();
            FontResource?.Dispose();
            Video?.Dispose();

            SDL.SDL_Quit();
        }

        public void Update()
        {
            Audio?.Update();
        }

        public int GetRealTime()
        {
            return (int)(SDL.SDL_GetTicks() - _initialTime);
        }

        public void Sleep(int duration)
        {
            SDL.SDL_Delay((uint)duration);
        }
    }
}
