using System;
using Zelda.Game.Lowlevel;
using Zelda.Game.Script;

namespace Zelda.Game
{
    static class Framework
    {
        static Surface _rootSurface;
        static ScriptSurface _rootScriptSurface;
        static Game _nextGame;

        public static bool Exiting { get; set; }
        public static Game Game { get; private set; }

        public static int Run(Arguments args)
        {
            Engine.Initialize(args);

            LoadModProperties();

            CurrentMod.Initialize();

            _rootSurface = Surface.Create(Video.ModSize);
            _rootSurface.IsSoftwareDestination = false;

            _rootScriptSurface = new ScriptSurface(_rootSurface);

            ScriptContext.Initialize();
            
            Video.ShowWindow();

            Loop();

            if (Game != null)
                Game.Stop();

            ScriptContext.Exit();
            Engine.Quit();

            return 0;
        }

        // 유저가 프로그램에 대한 종료 요청을 보내기 전까지 메인 루프를 실행한다.
        // 메인 루프는 게임 시간을 컨트롤하고 반복해서 월드를 갱신, 화면을 그린다.
        static void Loop()
        {
            uint lastFrameDate = Engine.GetRealTime();
            uint lag = 0;               // 따라잡아야 하는 게임 시간
            uint timeDropped = 0;      // 따라잡지 못한 시간

            while (!Exiting)
            {
                // 마지막 이터레이션 시간 측정
                uint now = Engine.GetRealTime() - timeDropped;
                uint lastFrameDuration = now - lastFrameDate;
                lastFrameDate = now;
                lag += lastFrameDuration;
                // 이제 lag은 게임 시각이 실제 시각과 비교해서 얼마나 늦었는지를 의미.

                if (lag >= 200)
                {
                    // 매우 큰 랙. 따라잡는 대신 가짜 실제 시각을 사용.
                    timeDropped += lag - Engine.TimeStep;
                    lag = Engine.TimeStep;
                    lastFrameDate = Engine.GetRealTime() - timeDropped;
                }

                // 1. 입력 이벤트를 감지하고 처리
                CheckInput();

                // 2. 월드를 한번, 혹은 시스템이 느릴 경우 따라잡기 위해 여러번 갱신 (그리기는 스킵).
                int numUpdates = 0;
                while (lag >= Engine.TimeStep &&
                       numUpdates < 10 && // 매우 느린 시스템에서도 적어도 가끔은 그리기 위해
                       !Exiting)
                {
                    Update();
                    lag -= Engine.TimeStep;
                    ++numUpdates;
                }

                // 3. 화면 그리기
                if (numUpdates > 0)
                    Draw();

                // 4. CPU와 GPU 사이클을 절약하기 위해, 가능하면 sleep.
                lastFrameDuration = (Engine.GetRealTime() - timeDropped) - lastFrameDate;
                if (lastFrameDuration < Engine.TimeStep)
                    Engine.Sleep(Engine.TimeStep - lastFrameDuration);
            }
        }

        static void Update()
        {
            Game?.Update();

            ScriptContext.Update();
            Engine.Update();

            if (_nextGame != Game)
            {
                Game = _nextGame;

                if (Game != null)
                {
                    Game.Start();
                }
                else
                {
                    ScriptContext.Exit();
                    ScriptContext.Initialize();
                }
            }
        }

        static void CheckInput()
        {
            var inputEvent = InputEvent.GetEvent();
            while (inputEvent != null)
            {
                NotifyInput(inputEvent);
                inputEvent = InputEvent.GetEvent();
            }
        }

        static void NotifyInput(InputEvent inputEvent)
        {
            if (inputEvent.IsWindowClosing)
                Exiting = true;

            var handled = ScriptContext.MainOnInput(inputEvent);
            if (!handled && Game != null)
                Game.NotifyInput(inputEvent);
        }

        static void Draw()
        {
            _rootSurface.Clear();

            Game?.Draw(_rootSurface);
            ScriptContext.MainOnDraw(_rootScriptSurface);
            Video.Render(_rootSurface);
        }

        static void LoadModProperties()
        {
            // 모드 속성 파일을 읽습니다
            var fileName = "mod.xml";
            var modProperties = new ModProperties();
            modProperties.ImportFromModFile(fileName);

            CheckVersionCompatibility(modProperties.ZeldaVersion);
            ModFiles.SetModWriteDir(modProperties.ModWriteDir);
            if (!modProperties.TitleBar.IsNullOrWhiteSpace())
                Video.WindowTitle = modProperties.TitleBar;

            Video.SetModSizeRange(
                modProperties.NormalModSize,
                modProperties.MinModSize,
                modProperties.MaxModSize);
        }

        static void CheckVersionCompatibility(string zeldaRequiredVersion)
        {
            if (string.IsNullOrWhiteSpace(zeldaRequiredVersion))
                Debug.Die("No Zelda version is specified in your mod.xml file!");

            var requiredVersion = Version.Parse(zeldaRequiredVersion);
            if (requiredVersion.Major != ZeldaVersion.Version.Major ||
                requiredVersion.Minor != ZeldaVersion.Version.Minor)
            {
                string msg = "This mod is made for Zelda {0}.{1}".F(requiredVersion.Major, requiredVersion.Minor);
                msg += ".x but you are running Zelda {0}".F(ZeldaVersion.Version.ToString());
                Debug.Die(msg);
            }
        }

        internal static void SetGame(Game game)
        {
            _nextGame = game;
        }

        public static void SetResetting()
        {
            if (Game != null)
                Game.Stop();
            
            SetGame(null);
        }
    }
}
