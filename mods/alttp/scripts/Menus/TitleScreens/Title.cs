using System;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus.TitleScreens
{
    class TitleScreen : ScriptMenu
    {
        readonly bool _debugEnabled;

        IPhase _phase;
        ScriptSurface _surface;

        public TitleScreen(bool debugEnabled)
        {
            _debugEnabled = debugEnabled;
        }

        protected override void OnStarted()
        {
            _surface = ScriptSurface.Create(320, 240);
            
            // 0.3초 동안 검정 스크린을 유지합니다.
            _phase = new BlackPhase();
            _phase.Finished += (_, e) => PhaseZsPresents();
        }

        void PhaseZsPresents()
        {
            _phase = new ZsPresentsPhase(_surface);
            _phase.Finished += (_, e) => 
            {
                _surface.FadeOut(10);
                ScriptTimer.Start(this, 700, (Action)PhaseTitle);
            };
        }

        void PhaseTitle()
        {
            _phase = new TitlePhase(_surface);
            _phase.Finished += (_, e) => FinishTitle();
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            _phase.OnDraw(dstSurface);
            _surface.Draw(dstSurface, dstSurface.Width / 2 - 160, dstSurface.Height / 2 - 120);
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
            ScriptAudio.StopMusic();
            ScriptMenu.Stop(this);
        }
    }
}
