using System;
using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.TitleScreens
{
    class ZsPresentsPhase : IPhase
    {
        readonly TitleScreen _titleScreen;
        readonly Surface _zsPresentsImg;
        readonly Point _zsPresentsPos;

        public event EventHandler Finished;

        public ZsPresentsPhase(TitleScreen titleScreen)
        {
            _titleScreen = titleScreen;

            _zsPresentsImg = Surface.Create("title_screen_initialization.png", true, Surface.ImageDirectory.Language);

            _zsPresentsPos = new Point(160 - _zsPresentsImg.Width / 2, 120 - _zsPresentsImg.Height / 2);
            Core.Audio?.PlaySound("intro");

            Timer.Start(_titleScreen, 2000, () => Finished?.Invoke(this, EventArgs.Empty));
        }

        public void OnDraw(Surface dstSurface)
        {
            _zsPresentsImg.Draw(_titleScreen.Surface, _zsPresentsPos);
        }

        public bool TryFinishTitle()
        {
            return false;
        }
    }
}
