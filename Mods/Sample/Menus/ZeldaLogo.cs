using System;
using Zelda.Game.Script;

namespace Sample.Menus
{
    class ZeldaLogo : Menu
    {
        public event EventHandler Started;

        protected override void OnStarted()
        {
            if (Started != null)
                Started(this, EventArgs.Empty);
        }
    }
}
