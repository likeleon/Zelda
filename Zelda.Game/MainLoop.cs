using System;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game
{
    public class MainLoop : IDisposable
    {
        public bool Exiting { get; set; }

        Game _game;
        internal Game Game
        {
            get { return _game; }
        }

        EngineSystem _engineSystem = new EngineSystem();
        internal EngineSystem EngineSystem
        {
            get { return _engineSystem; }
        }

        readonly CurrentMod _currentMod;
        internal CurrentMod CurrentMod
        {
            get { return _currentMod; }
        }

        readonly ScriptContext _scriptContext;
        readonly Surface _rootSurface;
        Game _nextGame;

        public MainLoop(Arguments args)
        {
            _engineSystem.Initialize(args);

            LoadModProperties();

            _currentMod = new CurrentMod(_engineSystem.ModFiles);
            _currentMod.Initialize();

            _rootSurface = Surface.Create(_engineSystem.Video.GameSize);

            _scriptContext = new ScriptContext(this);
            _scriptContext.Initialize();
            
            _engineSystem.Video.ShowWindow();
        }

        public void Dispose()
        {
            if (_game != null)
                _game.Stop();

            _scriptContext.Exit();
            _engineSystem.Quit();
        }

        // 유저가 프로그램에 대한 종료 요청을 보내기 전까지 메인 루프를 실행한다.
        // 메인 루프는 게임 시간을 컨트롤하고 반복해서 월드를 갱신, 화면을 그린다.
        public void Run()
        {
            int lastFrameDate = _engineSystem.GetRealTime();
            int lag = 0;               // 따라잡아야 하는 게임 시간
            int timeDropped = 0;      // 따라잡지 못한 시간

            while (!Exiting)
            {
                // 마지막 이터레이션 시간 측정
                int now = _engineSystem.GetRealTime() - timeDropped;
                int lastFrameDuration = now - lastFrameDate;
                lastFrameDate = now;
                lag += lastFrameDuration;
                // 이제 lag은 게임 시각이 실제 시각과 비교해서 얼마나 늦었는지를 의미.

                if (lag >= 200)
                {
                    // 매우 큰 랙. 따라잡는 대신 가짜 실제 시각을 사용.
                    timeDropped += lag - _engineSystem.TimeStep;
                    lag = _engineSystem.TimeStep;
                    lastFrameDate = _engineSystem.GetRealTime() - timeDropped;
                }

                // 1. 입력 이벤트를 감지하고 처리
                CheckInput();

                // 2. 월드를 한번, 혹은 시스템이 느릴 경우 따라잡기 위해 여러번 갱신 (그리기는 스킵).
                int numUpdates = 0;
                while (lag >=- _engineSystem.TimeStep &&
                       numUpdates < 10 && // 매우 느린 시스템에서도 적어도 가끔은 그리기 위해
                       !Exiting)
                {
                    Update();
                    lag -= _engineSystem.TimeStep;
                    ++numUpdates;
                }

                // 3. 화면 그리기
                if (numUpdates > 0)
                    Draw();

                // 4. CPU와 GPU 사이클을 절약하기 위해, 가능하면 sleep.
                lastFrameDuration = (_engineSystem.GetRealTime() - timeDropped) - lastFrameDate;
                if (lastFrameDuration < _engineSystem.TimeStep)
                    _engineSystem.Sleep(_engineSystem.TimeStep - lastFrameDuration);
            }
        }

        private void Update()
        {
            if (_game != null)
                _game.Update();

            _engineSystem.Update();

            if (_nextGame != _game)
            {
                _game = _nextGame;

                if (_game != null)
                {
                    _game.Start();
                }
                else
                {
                    _scriptContext.Exit();
                    _scriptContext.Initialize();
                }
            }
        }

        private void CheckInput()
        {
            Input.Event inputEvent = _engineSystem.Input.GetEvent();
            while (inputEvent != null)
            {
                NotifyInput(inputEvent);
                inputEvent = _engineSystem.Input.GetEvent();
            }
        }

        private void NotifyInput(Input.Event inputEvent)
        {
            if (inputEvent.IsWindowClosing)
                Exiting = true;

            if (_game != null)
                _game.NotifyInput(inputEvent);
        }

        private void Draw()
        {
            _rootSurface.Clear();

            if (_game != null)
                _game.Draw(_rootSurface);

            _engineSystem.Video.Render(_rootSurface);
        }

        private void LoadModProperties()
        {
            // 모드 속성 파일을 읽습니다
            string fileName = "Mod.xml";

            ModProperties modProperties = null;
            try
            {
                using (Stream buffer = _engineSystem.ModFiles.DataFileRead(fileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ModProperties));
                    modProperties = (ModProperties)serializer.Deserialize(buffer);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to load Mod.xml: " + ex.Message);
            }

            CheckVersionCompatibility(modProperties.ZeldaVersion);
            _engineSystem.ModFiles.SetModWriteDir(modProperties.ModWriteDir);
            if (!String.IsNullOrWhiteSpace(modProperties.TitleBar))
                _engineSystem.Video.WindowTitle = modProperties.TitleBar;

            _engineSystem.Video.DetermineGameSize();
        }

        private void CheckVersionCompatibility(string zeldaRequiredVersion)
        {
            if (String.IsNullOrWhiteSpace(zeldaRequiredVersion))
                throw new InvalidDataException("No Zelda version is specified in your Mod.xml file!");

            Version requiredVersion = Version.Parse(zeldaRequiredVersion);
            if (requiredVersion.Major != _engineSystem.ZeldaVersion.Major ||
                requiredVersion.Minor != _engineSystem.ZeldaVersion.Minor)
            {
                string msg = "This mod is made for Zelda " + requiredVersion.Major + "." + requiredVersion.Minor
                           + ".x but you are running Zelda" + _engineSystem.ZeldaVersion.ToString();
                throw new InvalidDataException(msg);
            }
        }

        internal void SetGame(Game game)
        {
            _nextGame = game;
        }
    }
}
