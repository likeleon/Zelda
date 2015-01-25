using System;
using EngineSystem = Zelda.Game.Engine.System;

namespace Zelda.Game
{
    public class MainLoop : IDisposable
    {
        public bool Exiting { get; set; }

        public MainLoop(Arguments args)
        {
            EngineSystem.Initialize(args);
        }

        public void Dispose()
        {
            EngineSystem.Quit();
        }

        // 유저가 프로그램에 대한 종료 요청을 보내기 전까지 메인 루프를 실행한다.
        // 메인 루프는 게임 시간을 컨트롤하고 반복해서 월드를 갱신, 화면을 그린다.
        public void Run()
        {
            uint last_frame_date = EngineSystem.GetRealTime();
            uint lag = 0;               // 따라잡아야 하는 게임 시간
            uint time_dropped = 0;      // 따라잡지 못한 시간

            while (!Exiting)
            {
                // 마지막 이터레이션 시간 측정
                uint now = EngineSystem.GetRealTime() - time_dropped;
                uint last_frame_duration = now - last_frame_date;
                last_frame_date = now;
                lag += last_frame_duration;
                // 이제 lag은 게임 시각이 실제 시각과 비교해서 얼마나 늦었는지를 의미.

                if (lag >= 200)
                {
                    // 매우 큰 랙. 따라잡는 대신 가짜 실제 시각을 사용.
                    time_dropped += lag - EngineSystem.TimeStep;
                    lag = EngineSystem.TimeStep;
                    last_frame_date = EngineSystem.GetRealTime() - time_dropped;
                }

                // 1. 입력 이벤트를 감지하고 처리
                CheckInput();

                // 2. 월드를 한번, 혹은 시스템이 느릴 경우 따라잡기 위해 여러번 갱신 (그리기는 스킵).
                int num_updates = 0;
                while (lag >=- EngineSystem.TimeStep &&
                       num_updates < 10 && // 매우 느린 시스템에서도 적어도 가끔은 그리기 위해
                       !Exiting)
                {
                    Update();
                    lag -= EngineSystem.TimeStep;
                    ++num_updates;
                }

                // 3. 화면 그리기
                if (num_updates > 0)
                    Draw();

                // 4. CPU와 GPU 사이클을 절약하기 위해, 가능하면 sleep.
                last_frame_duration = (EngineSystem.GetRealTime() - time_dropped) - last_frame_date;
                if (last_frame_duration < EngineSystem.TimeStep)
                    EngineSystem.Sleep(EngineSystem.TimeStep - last_frame_duration);
            }
        }

        private void Update()
        {
            EngineSystem.Update();
        }

        private void CheckInput()
        {
        }

        private void Draw()
        {
        }
    }
}
