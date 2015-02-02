﻿using System;
using Zelda.Game.Engine;
using EngineSystem = Zelda.Game.Engine.System;

namespace Zelda.Game
{
    public class MainLoop : IDisposable
    {
        public bool Exiting { get; set; }

        private readonly EngineSystem _system = new EngineSystem();
        private readonly Surface _rootSurface;

        public MainLoop(Arguments args)
        {
            _system.Initialize(args);

            LoadAssetProperties();

            _rootSurface = Surface.Create(_system.Video.GameSize);
            
            _system.Video.ShowWindow();
        }

        public void Dispose()
        {
            _system.Quit();
        }

        // 유저가 프로그램에 대한 종료 요청을 보내기 전까지 메인 루프를 실행한다.
        // 메인 루프는 게임 시간을 컨트롤하고 반복해서 월드를 갱신, 화면을 그린다.
        public void Run()
        {
            uint lastFrameDate = _system.GetRealTime();
            uint lag = 0;               // 따라잡아야 하는 게임 시간
            uint timeDropped = 0;      // 따라잡지 못한 시간

            while (!Exiting)
            {
                // 마지막 이터레이션 시간 측정
                uint now = _system.GetRealTime() - timeDropped;
                uint lastFrameDuration = now - lastFrameDate;
                lastFrameDate = now;
                lag += lastFrameDuration;
                // 이제 lag은 게임 시각이 실제 시각과 비교해서 얼마나 늦었는지를 의미.

                if (lag >= 200)
                {
                    // 매우 큰 랙. 따라잡는 대신 가짜 실제 시각을 사용.
                    timeDropped += lag - _system.TimeStep;
                    lag = _system.TimeStep;
                    lastFrameDate = _system.GetRealTime() - timeDropped;
                }

                // 1. 입력 이벤트를 감지하고 처리
                CheckInput();

                // 2. 월드를 한번, 혹은 시스템이 느릴 경우 따라잡기 위해 여러번 갱신 (그리기는 스킵).
                int numUpdates = 0;
                while (lag >=- _system.TimeStep &&
                       numUpdates < 10 && // 매우 느린 시스템에서도 적어도 가끔은 그리기 위해
                       !Exiting)
                {
                    Update();
                    lag -= _system.TimeStep;
                    ++numUpdates;
                }

                // 3. 화면 그리기
                if (numUpdates > 0)
                    Draw();

                // 4. CPU와 GPU 사이클을 절약하기 위해, 가능하면 sleep.
                lastFrameDuration = (_system.GetRealTime() - timeDropped) - lastFrameDate;
                if (lastFrameDuration < _system.TimeStep)
                    _system.Sleep(_system.TimeStep - lastFrameDuration);
            }
        }

        private void Update()
        {
            _system.Update();
        }

        private void CheckInput()
        {
            Input.Event inputEvent = _system.Input.GetEvent();
            while (inputEvent != null)
            {
                NotifyInput(inputEvent);
                inputEvent = _system.Input.GetEvent();
            }
        }

        private void NotifyInput(Input.Event inputEvent)
        {
            if (inputEvent.IsWindowClosing)
                Exiting = true;
        }

        private void Draw()
        {
            _rootSurface.Clear();

            _system.Video.Render(_rootSurface);
        }

        private void LoadAssetProperties()
        {
            _system.Video.DetermineGameSize();
        }
    }
}
