using System;
using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.TitleScreens
{
    class BlackPhase : IPhase
    {
        public event EventHandler Finished;

        public BlackPhase(TitleScreen titleScreen)
        {
            // 0.3초의 시간 동안 사운드 이펙트들을 미리 로딩합니다.
            Core.Audio?.PreloadSounds();

            Timer.Start(titleScreen, 300, () => Finished?.Invoke(this, EventArgs.Empty));
        }

        public void OnDraw(Surface dstSurface)
        {
        }

        public bool TryFinishTitle()
        {
            return false;
        }
    }
}
