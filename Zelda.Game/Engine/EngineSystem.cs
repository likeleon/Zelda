using SDL2;
using System;
using System.Reflection;

namespace Zelda.Game.Engine
{
    class EngineSystem
    {
        private readonly ModFiles _modFiles = new ModFiles();
        public ModFiles ModFiles
        {
            get { return _modFiles; }
        }

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

        private int _initialTime = 0;       // 초기화 시점의 실제 시각, 밀리초
        private int _ticks = 0;             // 게임 시각, 밀리초
     
        public readonly int TimeStep = 10;  // 업데이트시마다 추가될 게임 시간, 밀리초
        
        public string Os
        {
            get { return SDL.SDL_GetPlatform(); }
        }

        public Version ZeldaVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public void Initialize(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = (int)GetRealTime();

            _modFiles.Initialize(args);
            InitializeLog();
            _input.Initialize();
            _video.Initialize(args, ZeldaVersion.ToString());
        }

        private void InitializeLog()
        {
            Log.Initialize(_modFiles.BaseWriteDir + "/" + _modFiles.ZeldaWriteDir + "/");

            Log.AddChannel("Perf", "Perf.log");
            Log.AddChannel("Debug", "Debug.log");
            Log.AddChannel("Sound", "Sound.log");
            Log.AddChannel("Graphics", "Graphics.log");
        }

        public void Quit()
        {
            _input.Quit();
            _video.Quit();
            _modFiles.Quit();

            SDL.SDL_Quit();
        }

        public void Update()
        {
            _ticks += TimeStep;
        }

        public int GetRealTime()
        {
            return (int)SDL.SDL_GetTicks() - _initialTime;
        }

        public void Sleep(int duration)
        {
            SDL.SDL_Delay((uint)duration);
        }
    }
}
