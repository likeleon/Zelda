using System;
using System.IO;
using Zelda.Game.Engine;
using ScriptContext = Zelda.Game.Script.ScriptContext;
using ScriptSurface = Zelda.Game.Script.Surface;

namespace Zelda.Game
{
    class MainLoop : IDisposable
    {
        public bool Exiting { get; set; }

        Game _game;
        public Game Game
        {
            get { return _game; }
        }

        bool IsResetting
        {
            get { return _game != null && _nextGame == null; }
        }

        readonly Surface _rootSurface;
        readonly ScriptSurface _rootScriptSurface;
        Game _nextGame;

        public MainLoop(Arguments args)
        {
            EngineSystem.Initialize(args);

            LoadModProperties();

            CurrentMod.Initialize();

            _rootSurface = Surface.Create(Video.ModSize);
            _rootSurface.SoftwareDestination = false;

            _rootScriptSurface = new ScriptSurface(_rootSurface);

            ScriptContext.Initialize(this);
            
            Video.ShowWindow();
        }

        public void Dispose()
        {
            if (_game != null)
                _game.Stop();

            ScriptContext.Exit();
            EngineSystem.Quit();
        }

        // 유저가 프로그램에 대한 종료 요청을 보내기 전까지 메인 루프를 실행한다.
        // 메인 루프는 게임 시간을 컨트롤하고 반복해서 월드를 갱신, 화면을 그린다.
        public void Run()
        {
            uint lastFrameDate = EngineSystem.GetRealTime();
            uint lag = 0;               // 따라잡아야 하는 게임 시간
            uint timeDropped = 0;      // 따라잡지 못한 시간

            while (!Exiting)
            {
                // 마지막 이터레이션 시간 측정
                uint now = EngineSystem.GetRealTime() - timeDropped;
                uint lastFrameDuration = now - lastFrameDate;
                lastFrameDate = now;
                lag += lastFrameDuration;
                // 이제 lag은 게임 시각이 실제 시각과 비교해서 얼마나 늦었는지를 의미.

                if (lag >= 200)
                {
                    // 매우 큰 랙. 따라잡는 대신 가짜 실제 시각을 사용.
                    timeDropped += lag - EngineSystem.TimeStep;
                    lag = EngineSystem.TimeStep;
                    lastFrameDate = EngineSystem.GetRealTime() - timeDropped;
                }

                // 1. 입력 이벤트를 감지하고 처리
                CheckInput();

                // 2. 월드를 한번, 혹은 시스템이 느릴 경우 따라잡기 위해 여러번 갱신 (그리기는 스킵).
                int numUpdates = 0;
                while (lag >= EngineSystem.TimeStep &&
                       numUpdates < 10 && // 매우 느린 시스템에서도 적어도 가끔은 그리기 위해
                       !Exiting)
                {
                    Update();
                    lag -= EngineSystem.TimeStep;
                    ++numUpdates;
                }

                // 3. 화면 그리기
                if (numUpdates > 0)
                    Draw();

                // 4. CPU와 GPU 사이클을 절약하기 위해, 가능하면 sleep.
                lastFrameDuration = (EngineSystem.GetRealTime() - timeDropped) - lastFrameDate;
                if (lastFrameDuration < EngineSystem.TimeStep)
                    EngineSystem.Sleep(EngineSystem.TimeStep - lastFrameDuration);
            }
        }

        private void Update()
        {
            if (_game != null)
                _game.Update();

            ScriptContext.Update();
            EngineSystem.Update();

            if (_nextGame != _game)
            {
                _game = _nextGame;

                if (_game != null)
                {
                    _game.Start();
                }
                else
                {
                    ScriptContext.Exit();
                    ScriptContext.Initialize(this);
                }
            }
        }

        private void CheckInput()
        {
            InputEvent inputEvent = InputEvent.GetEvent();
            while (inputEvent != null)
            {
                NotifyInput(inputEvent);
                inputEvent = InputEvent.GetEvent();
            }
        }

        private void NotifyInput(InputEvent inputEvent)
        {
            if (inputEvent.IsWindowClosing)
                Exiting = true;

            bool handled = ScriptContext.MainOnInput(inputEvent);
            if (!handled && _game != null)
                _game.NotifyInput(inputEvent);
        }

        private void Draw()
        {
            _rootSurface.Clear();

            if (_game != null)
                _game.Draw(_rootSurface);
            ScriptContext.MainOnDraw(_rootScriptSurface);
            Video.Render(_rootSurface);
        }

        private void LoadModProperties()
        {
            // 모드 속성 파일을 읽습니다
            string fileName = "Mod.xml";

            ModProperties modProperties = ModProperties.ImportFrom(fileName);

            CheckVersionCompatibility(modProperties.ZeldaVersion);
            ModFiles.SetModWriteDir(modProperties.ModWriteDir);
            if (!String.IsNullOrWhiteSpace(modProperties.TitleBar))
                Video.WindowTitle = modProperties.TitleBar;

            Video.SetModSizeRange(
                modProperties.NormalModSize,
                modProperties.MinModSize,
                modProperties.MaxModSize);
        }

        private void CheckVersionCompatibility(string zeldaRequiredVersion)
        {
            if (String.IsNullOrWhiteSpace(zeldaRequiredVersion))
                throw new InvalidDataException("No Zelda version is specified in your Mod.xml file!");

            Version requiredVersion = Version.Parse(zeldaRequiredVersion);
            if (requiredVersion.Major != EngineSystem.ZeldaVersion.Major ||
                requiredVersion.Minor != EngineSystem.ZeldaVersion.Minor)
            {
                string msg = "This mod is made for Zelda {0}.{1}".F(requiredVersion.Major, requiredVersion.Minor);
                msg += ".x but you are running Zelda {0}".F(EngineSystem.ZeldaVersion.ToString());
                throw new InvalidDataException(msg);
            }
        }

        internal void SetGame(Game game)
        {
            _nextGame = game;
        }

        public void SetResetting()
        {
            if (_game != null)
                _game.Stop();
            
            SetGame(null);
        }
    }
}
