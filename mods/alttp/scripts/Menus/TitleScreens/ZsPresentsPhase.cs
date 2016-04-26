using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Alttp.Menus.TitleScreens
{
    class ZsPresentsPhase : IPhase
    {
        readonly TitleScreen _titleScreen;
        readonly ScriptSurface _zsPresentsImg;
        readonly Point _zsPresentsPos;

        public event EventHandler Finished = delegate { };

        public ZsPresentsPhase(TitleScreen titleScreen)
        {
            _titleScreen = titleScreen;

            _zsPresentsImg = ScriptSurface.Create("title_screen_initialization.png", true);

            _zsPresentsPos = new Point(160 - _zsPresentsImg.Width / 2, 120 - _zsPresentsImg.Height / 2);
            ScriptAudio.PlaySound("intro");

            ScriptTimer.Start(_titleScreen, 2000, () => Finished(this, EventArgs.Empty));
        }

        public void OnDraw(ScriptSurface dstSurface)
        {
            _zsPresentsImg.Draw(_titleScreen.Surface, _zsPresentsPos);
        }

        public bool TryFinishTitle()
        {
            return false;
        }
    }
}
