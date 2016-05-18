using SDL2;
using System;

namespace Zelda.Game.LowLevel
{
    class Platform : IDisposable
    {
        public string Os => SDL.SDL_GetPlatform();
        public Video Video { get; }
        public Audio Audio { get; }

        readonly uint _initialTime;       // 초기화 시점의 실제 시각, 밀리초

        public Platform(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = GetRealTime();

            Audio = new Audio(args);
            InputEvent.Initialize();
            Video = new Video(args, ZeldaVersion.Version.ToString());
            FontResource.Initialize();
            Sprite.Initialize();
        }

        public void Dispose()
        {
            InputEvent.Quit();
            if (Audio != null)
                Audio.Dispose();
            Sprite.Quit();
            FontResource.Quit();
            if (Video != null)
                Video.Dispose();

            SDL.SDL_Quit();
        }

        public void Update()
        {
            Audio.Update();
        }

        public uint GetRealTime()
        {
            return (SDL.SDL_GetTicks() - _initialTime);
        }

        public void Sleep(uint duration)
        {
            SDL.SDL_Delay(duration);
        }
    }
}
