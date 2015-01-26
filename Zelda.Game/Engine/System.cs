using SDL2;

namespace Zelda.Game.Engine
{
    public static class System
    {
        private static uint _initialTime = 0;      // 초기화 시점의 실제 시각, 밀리초
        private static uint _ticks = 0;             // 게임 시각, 밀리초
        public static readonly uint TimeStep = 10;  // 업데이트시마다 추가될 게임 시간, 밀리초

        public static string Os
        {
            get { return SDL.SDL_GetPlatform(); }
        }

        public static void Initialize(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = GetRealTime();

            InputEvent.Initialize();
        }

        public static void Quit()
        {
            InputEvent.Quit();

            SDL.SDL_Quit();
        }

        public static void Update()
        {
            _ticks += TimeStep;
        }

        public static uint GetRealTime()
        {
            return SDL.SDL_GetTicks() - _initialTime;
        }

        public static void Sleep(uint duration)
        {
            SDL.SDL_Delay(duration);
        }
    }
}
