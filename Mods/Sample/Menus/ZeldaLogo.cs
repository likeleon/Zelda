using Zelda.Game.Script;

namespace Sample.Menus
{
    class ZeldaLogo : Menu
    {
        readonly Surface _surface;
        readonly Sprite _logo;

        public ZeldaLogo()
        {
            _surface = Surface.Create(400, 240);
            _logo = Sprite.Create("Menus/zelda_logo.png", 400, 240);
        }

        protected override void OnStarted()
        {
            _surface.Opacity = 255;
            
            RebuildSurface();
        }
        
        void RebuildSurface()
        {
            _surface.Clear();
            _logo.Draw(_surface, 0, 0);
        }

        protected override void OnDraw(Surface screen)
        {
            _surface.Draw(screen, 0, 0);
        }
    }
}
