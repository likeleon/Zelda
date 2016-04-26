using SDL2;
using System;

namespace Zelda.Game.Lowlevel
{
    class Platform : IDisposable
    {
        readonly uint _initialTime = 0;       // 초기화 시점의 실제 시각, 밀리초

        public static uint TimeStep { get; } = 10;  // 업데이트시마다 추가될 게임 시간, 밀리초

        public string Os { get { return SDL.SDL_GetPlatform(); } }
        public uint Now { get; private set; }

        internal Platform(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = GetRealTime();

            ModFiles.Initialize(args);
            Sound.Initialize(args);
            InputEvent.Initialize();
            Video.Initialize(args, ZeldaVersion.Version.ToString());
            FontResource.Initialize();
            Sprite.Initialize();
        }

        public void Dispose()
        {
            InputEvent.Quit();
            Sound.Quit();
            Sprite.Quit();
            FontResource.Quit();
            Video.Quit();
            ModFiles.Quit();

            SDL.SDL_Quit();
        }

        public void Update()
        {
            Now += TimeStep;
            Sound.Update();
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
