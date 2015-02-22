using System;
using Zelda.Game.Script;

namespace Sample.Menus
{
    class ZeldaLogo : Menu
    {
        public event EventHandler Started;

        readonly Surface _surface;

        public ZeldaLogo()
        {
            _surface = Surface.Create(201, 48);
        }

        protected override void OnStarted()
        {
            if (Started != null)
                Started(this, EventArgs.Empty);
        }
    }
}
