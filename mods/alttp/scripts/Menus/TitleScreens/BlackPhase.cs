using System;
using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp.Menus.TitleScreens
{
    class BlackPhase : IPhase
    {
        public event EventHandler Finished = delegate {};

        public BlackPhase(TitleScreen titleScreen)
        {
            // 0.3초의 시간 동안 사운드 이펙트들을 미리 로딩합니다.
            Core.Audio?.PreloadSounds();

            Timer.Start(titleScreen, 300, () => Finished(this, EventArgs.Empty));
        }

        public void OnDraw(ScriptSurface dstSurface)
        {
        }

        public bool TryFinishTitle()
        {
            return false;
        }
    }
}
