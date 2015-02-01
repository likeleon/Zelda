using SDL2;

namespace Zelda.Game.Engine
{
    public class System
    {
        private readonly Video _video = new Video();
        public Video Video
        {
            get { return _video; }
        }

        private readonly Input _input = new Input();
        public Input Input
        {
            get { return _input; }
        }

        private uint _initialTime = 0;       // 초기화 시점의 실제 시각, 밀리초
        private uint _ticks = 0;             // 게임 시각, 밀리초
     
        public readonly uint TimeStep = 10;  // 업데이트시마다 추가될 게임 시간, 밀리초
        
        public string Os
        {
            get { return SDL.SDL_GetPlatform(); }
        }

        public void Initialize(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = GetRealTime();

            _input.Initialize();
            _video.Initialize(args);
        }

        public void Quit()
        {
            _input.Quit();
            _video.Quit();

            SDL.SDL_Quit();
        }

        public void Update()
        {
            _ticks += TimeStep;
        }

        public uint GetRealTime()
        {
            return SDL.SDL_GetTicks() - _initialTime;
        }

        public void Sleep(uint duration)
        {
            SDL.SDL_Delay(duration);
        }
    }
}
