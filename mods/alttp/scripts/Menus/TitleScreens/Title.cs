using System;
using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.TitleScreens
{
    class TitleScreen : Menu
    {
        readonly bool _debugEnabled;

        IPhase _phase;
        public Surface Surface { get; private set; }

        public TitleScreen(bool debugEnabled)
        {
            _debugEnabled = debugEnabled;
        }

        protected override void OnStarted()
        {
            Surface = Surface.Create(320, 240, true);
            
            // 0.3초 동안 검정 스크린을 유지합니다.
            _phase = new BlackPhase(this);
            _phase.Finished += (_, e) => PhaseZsPresents();
        }

        void PhaseZsPresents()
        {
            _phase = new ZsPresentsPhase(this);
            _phase.Finished += (_, e) => 
            {
                Surface.FadeOut(10);
                Timer.Start(this, 700, (Action)PhaseTitle);
            };
        }

        void PhaseTitle()
        {
            _phase = new TitlePhase(this);
            _phase.Finished += (_, e) => FinishTitle();
        }

        protected override void OnDraw(Surface dstSurface)
        {
            _phase.OnDraw(dstSurface);
            Surface.Draw(dstSurface, dstSurface.Width / 2 - 160, dstSurface.Height / 2 - 120);
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            var handled = false;

            if (key == KeyboardKey.Escape)
            {
                Main.Exit();
                handled = true;
            }
            else if (key == KeyboardKey.Space || key == KeyboardKey.Return)
            {
                handled = _phase.TryFinishTitle();
            }
            else if (_debugEnabled)
            {
                if (key == KeyboardKey.LeftShift || key == KeyboardKey.RightShift)
                {
                    FinishTitle();
                    handled = true;
                }
            }

            return handled;
        }

        void FinishTitle()
        {
            Core.Audio?.StopMusic();
            Stop();
        }
    }
}
