using SDL2;
using System.ComponentModel;

namespace Zelda.Game.Lowlevel
{
    static class Engine
    {
        static uint _initialTime = 0;       // 초기화 시점의 실제 시각, 밀리초
        static uint _ticks = 0;             // 게임 시각, 밀리초

        public static readonly uint TimeStep = 10;  // 업데이트시마다 추가될 게임 시간, 밀리초

        public static string Os
        {
            get { return SDL.SDL_GetPlatform(); }
        }

        [Description("엔진 초기화 후 진행된 게임 시간, 밀리초")]
        public static uint Now
        {
            get { return _ticks; }
        }

        public static void Initialize(Arguments args)
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

        public static void Quit()
        {
            InputEvent.Quit();
            Sound.Quit();
            Sprite.Quit();
            FontResource.Quit();
            Video.Quit();
            ModFiles.Quit();

            SDL.SDL_Quit();
        }

        public static void Update()
        {
            _ticks += TimeStep;
            Sound.Update();
        }

        public static uint GetRealTime()
        {
            return (SDL.SDL_GetTicks() - _initialTime);
        }

        public static void Sleep(uint duration)
        {
            SDL.SDL_Delay(duration);
        }
    }
}
