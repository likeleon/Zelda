using SDL2;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Zelda.Game.Engine
{
    static class EngineSystem
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

        public static Version ZeldaVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static void Initialize(Arguments args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            _initialTime = GetRealTime();

            ModFiles.Initialize(args);
            InitializeLog();
            Input.Initialize();
            Video.Initialize(args, ZeldaVersion.ToString());
            Sprite.Initialize();
        }

        private static void InitializeLog()
        {
            Log.Initialize(ModFiles.BaseWriteDir + "/" + ModFiles.ZeldaWriteDir + "/");

            Log.AddChannel("Perf", "Perf.log");
            Log.AddChannel("Debug", "Debug.log");
            Log.AddChannel("Sound", "Sound.log");
            Log.AddChannel("Graphics", "Graphics.log");
        }

        public static void Quit()
        {
            Input.Quit();
            Sprite.Quit();
            Video.Quit();
            ModFiles.Quit();

            SDL.SDL_Quit();
        }

        public static void Update()
        {
            _ticks += TimeStep;
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
