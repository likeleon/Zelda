﻿using System;
using System.Linq;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public static class Core
    {
        public static Mod Mod { get; private set; }
        public static Video Video => Platform.Video;
        public static Audio Audio => Platform.Audio;
        public static Input Input => Platform.Input;
        public static Main Main { get; private set; }
        internal static FontResource FontResource => Platform.FontResource;
        internal static SpriteSystem SpriteSystem => Platform.SpriteSystem;

        internal static Game Game { get; private set; }
        internal static int Now { get; private set; }
        internal static Platform Platform { get; private set; }
        internal static bool Exiting { get; set; }

        static readonly int TimeStep = 10;  // 업데이트시마다 추가될 게임 시간, 밀리초

        static Surface _rootSurface;
        static Game _nextGame;

        internal static void Initialize(Arguments args)
        {
            Logger.Info("Zelda " + ZeldaVersion.Version);

            Platform = new Platform(args);

            var modPath = GetModPath(args);
            Logger.Info("Opening mod '{0}'".F(modPath));
            Mod = new Mod(args.ProgramName, modPath);

            LoadModProperties();

            _rootSurface = Surface.Create(Video.ModSize, false);
            _rootSurface.IsSoftwareDestination = false;

            PrepareGame();

            Video.ShowWindow();
        }

        static string GetModPath(Arguments args)
        {
            if (args.Args.Any() && !args.Args.Last().IsNullOrEmpty() && args.Args.Last()[0] != '-')
                return args.Args.Last();
            return Properties.Settings.Default.DefaultMod;
        }

        internal static int Run()
        {
            try
            {
                Loop();
            }
            finally
            {
                StopGame();

                if (Platform != null)
                    Platform.Dispose();

                Mod?.Dispose();
            }

            return 0;
        }

        static void PrepareGame()
        {
            var mainTypes = Mod.ObjectCreator.GetTypesImplementing<Main>();
            if (!mainTypes.Any())
                throw new Exception("'Main' based class not found");

            if (mainTypes.Count() > 1)
                throw new Exception("'Too many 'Main' based classes");

            Main = (Main)Mod.ObjectCreator.CreateBasic(mainTypes.First());
            Main.OnStarted();
        }

        static void UpdateGame()
        {
            Game?.Update();
            Drawable.UpdateDrawables();
            Menu.UpdateMenus();
            Timer.UpdateTimers();
            Main.OnUpdate();
        }

        static void StopGame()
        {
            Main.OnFinished();

            Game?.Stop();
            Menu.DestroyMenus();
            Timer.DestroyTimers();
            Drawable.DestroyDrawables();

            Main.Dispose();
            Main = null;
        }

        // 유저가 프로그램에 대한 종료 요청을 보내기 전까지 메인 루프를 실행한다.
        // 메인 루프는 게임 시간을 컨트롤하고 반복해서 월드를 갱신, 화면을 그린다.
        static void Loop()
        {
            int lastFrameDate = Platform.GetRealTime();
            int lag = 0;               // 따라잡아야 하는 게임 시간
            int timeDropped = 0;      // 따라잡지 못한 시간

            while (!Exiting)
            {
                // 마지막 이터레이션 시간 측정
                int now = Platform.GetRealTime() - timeDropped;
                int lastFrameDuration = now - lastFrameDate;
                lastFrameDate = now;
                lag += lastFrameDuration;
                // 이제 lag은 게임 시각이 실제 시각과 비교해서 얼마나 늦었는지를 의미.

                if (lag >= 200)
                {
                    // 매우 큰 랙. 따라잡는 대신 가짜 실제 시각을 사용.
                    timeDropped += lag - TimeStep;
                    lag = TimeStep;
                    lastFrameDate = Platform.GetRealTime() - timeDropped;
                }

                // 1. 입력 이벤트를 감지하고 처리
                CheckInput();

                // 2. 월드를 한번, 혹은 시스템이 느릴 경우 따라잡기 위해 여러번 갱신 (그리기는 스킵).
                int numUpdates = 0;
                while (lag >= TimeStep &&
                       numUpdates < 10 && // 매우 느린 시스템에서도 적어도 가끔은 그리기 위해
                       !Exiting)
                {
                    Update();
                    lag -= TimeStep;
                    ++numUpdates;
                }

                // 3. 화면 그리기
                if (numUpdates > 0)
                    Draw();

                // 4. CPU와 GPU 사이클을 절약하기 위해, 가능하면 sleep.
                lastFrameDuration = (Platform.GetRealTime() - timeDropped) - lastFrameDate;
                if (lastFrameDuration < TimeStep)
                    Platform.Sleep(TimeStep - lastFrameDuration);
            }
        }

        static void Update()
        {
            UpdateGame();

            Now += TimeStep;
            Platform.Update();

            if (_nextGame == Game)
                return;

            Game = _nextGame;
            if (Game != null)
                Game.Start();
            else
            {
                StopGame();
                PrepareGame();
            }
        }

        static void CheckInput()
        {
            var evt = Input.GetEvent();
            while (evt != null)
            {
                NotifyInput(evt);
                evt = Input.GetEvent();
            }
        }

        static void NotifyInput(InputEvent inputEvent)
        {
            if (inputEvent.IsWindowClosing)
                Exiting = true;

            var handled = Main.OnInput(inputEvent);
            if (!handled)
                handled = Menu.MenusOnInput(Main, inputEvent);
            if (!handled)
                Game?.NotifyInput(inputEvent);
        }

        static void Draw()
        {
            _rootSurface.Clear();

            Game?.Draw(_rootSurface);
            Main.OnDraw(_rootSurface);
            Menu.MenusOnDraw(Main, _rootSurface);
            Video.Render(_rootSurface);
        }

        static void LoadModProperties()
        {
            var properties = Mod.Properties;

            CheckVersionCompatibility(Version.Parse(properties.ZeldaVersion));

            if (!properties.TitleBar.IsNullOrEmpty())
                Video.WindowTitle = Mod.Properties.TitleBar;

            Video.SetModSizeRange(properties.NormalModSize, properties.MinModSize, properties.MaxModSize);
        }

        static void CheckVersionCompatibility(Version required)
        {
            if (required.Major != ZeldaVersion.Version.Major || required.Minor != ZeldaVersion.Version.Minor)
            {
                var msg = "This mod is made for Zelda {0}.{1}.x but you are running Zelda {0}"
                    .F(required.Major, required.Minor, ZeldaVersion.Version);
                throw new Exception(msg);
            }
        }

        internal static void SetGame(Game game)
        {
            _nextGame = game;
        }
    }
}
