using System;
using Zelda.Game.Script;

namespace Alttp.Menus.TitleScreens
{
    class BlackPhase : IPhase
    {
        public event EventHandler Finished = delegate {};

        public BlackPhase()
        {
            // 0.3초의 시간 동안 사운드 이펙트들을 미리 로딩합니다.
            ScriptAudio.PreloadSounds();

            ScriptTimer.Start(this, 300, () => Finished(this, EventArgs.Empty));
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
