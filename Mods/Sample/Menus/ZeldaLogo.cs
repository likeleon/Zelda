using System;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Sample.Menus
{
    class ZeldaLogo : Menu
    {
        readonly Surface _surface;
        readonly Sprite _title;
        readonly Sprite _subtitle;
        readonly Sprite _sun;
        readonly Sprite _sword;
        readonly Surface _blackSquare;
        int _animationStep;

        public ZeldaLogo()
        {
            _surface = Surface.Create(201, 48);
            
            _title = Sprite.Create("Menus/solarus_logo");
            _title.SetAnimation("title");

            _subtitle = Sprite.Create("Menus/solarus_logo");
            _subtitle.SetAnimation("subtitle");

            _sun = Sprite.Create("Menus/solarus_logo");
            _sun.SetAnimation("sun");

            _sword = Sprite.Create("Menus/solarus_logo");
            _sword.SetAnimation("sword");

            _blackSquare = Surface.Create(48, 15);
            _blackSquare.FillColor(Color.Black);

            Timer.Start(this, 5000, () =>
            {
                Console.WriteLine("Timer expired");
                return true;
            });
        }

        void RebuildSurface()
        {
            _surface.Clear();

            //if (_animationStep >= 1)
                _title.Draw(_surface, 0, 0);

            _sun.Draw(_surface, 0, 33);
            _blackSquare.Draw(_surface, 24, 33);
            _sword.Draw(_surface, 48, -48);

            if (_animationStep >= 2)
                _subtitle.Draw(_surface);
        }

        protected override void OnStarted()
        {
            _animationStep = 0;
            _surface.Opacity = 255;
            _sun.SetDirection(0);
            _sun.XY = new Point(0, 0);
            _sword.XY = new Point(0, 0);

            StartAnimation();
            RebuildSurface();
        }

        protected override void OnDraw(Surface screen)
        {
            _surface.Draw(screen, screen.Width / 2 - 100, screen.Height / 2 - 24);
        }

        void StartAnimation()
        {
            TargetMovement sunMovement = Movement.Create(MovementType.Target) as TargetMovement;
            sunMovement.SetSpeed(64);
            sunMovement.SetTarget(new Point(0, -33));
            sunMovement.PositionChanged += () => RebuildSurface();
        }
    }
}
