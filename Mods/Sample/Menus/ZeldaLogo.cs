using Zelda.Game.Script;

namespace Sample.Menus
{
    class ZeldaLogo : Menu
    {
        readonly Surface _surface;
        readonly Sprite _title;

        public ZeldaLogo()
        {
            _surface = Surface.Create(201, 48);
            
            _title = Sprite.Create("Menus/solarus_logo");
            _title.SetAnimation("title");
        }

        protected override void OnStarted()
        {
            _surface.Opacity = 255;
            
            RebuildSurface();
        }
        
        void RebuildSurface()
        {
            _surface.Clear();
            _title.Draw(_surface, 0, 0);
        }

        protected override void OnDraw(Surface screen)
        {
            _surface.Draw(screen, screen.Width / 2 - 100, screen.Height / 2 - 24);
        }
    }
}
