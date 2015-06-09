using System;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus.TitleScreens
{
    class ZsPresentsPhase : IPhase
    {
        readonly ScriptSurface _surface;
        readonly ScriptSurface _zsPresentsImg;
        readonly Point _zsPresentsPos;

        public event EventHandler Finished = delegate { };

        public ZsPresentsPhase(ScriptSurface surface)
        {
            _surface = surface;

            _zsPresentsImg = ScriptSurface.Create("title_screen_initialization.png", true);

            _zsPresentsPos = new Point(160 - _zsPresentsImg.Width / 2, 120 - _zsPresentsImg.Height / 2);
            ScriptAudio.PlaySound("intro");

            ScriptTimer.Start(this, 2000, () => Finished(this, EventArgs.Empty));
        }

        public void OnDraw(ScriptSurface dstSurface)
        {
            _zsPresentsImg.Draw(_surface, _zsPresentsPos);
        }

        public bool TryFinishTitle()
        {
            return false;
        }
    }
}
