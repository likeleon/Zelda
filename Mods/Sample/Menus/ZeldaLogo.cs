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
        Timer _timer;

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

            if (_animationStep >= 1)
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
            // 태양의 이동
            TargetMovement sunMovement = Movement.Create(MovementType.Target) as TargetMovement;
            sunMovement.SetSpeed(64);
            sunMovement.SetTarget(new Point(0, -33));
            sunMovement.PositionChanged += (o, e) => RebuildSurface();

            // 검의 이동
            TargetMovement swordMovement = Movement.Create(MovementType.Target) as TargetMovement;
            swordMovement.SetSpeed(96);
            swordMovement.SetTarget(new Point(-48, 48));
            swordMovement.PositionChanged += (o, e) => RebuildSurface();

            // 이동을 시작합니다
            sunMovement.Start(_sun, () =>
            {
                swordMovement.Start(_sword, () =>
                {
                    if (!IsStarted())
                    {
                        // 메뉴는 정지되었지만 이동은 계속된 경우
                        return;
                    }

                    if (_animationStep <= 0)
                    {
                        // 스텝 1을 시작합니다
                        Step1();

                        // 스텝 2를 위한 타이머를 생성합니다.
                        _timer = Timer.Start(this, 250, () =>
                        {
                            if (_animationStep <= 1)
                                Step2();
                            
                            return false;
                        });
                    }
                });
            });
        }

        void Step1()
        {
            _animationStep = 1;
            
            _sun.SetDirection(1);
            _sun.StopMovement();
            _sun.XY = new Point(0, -33);

            _sword.StopMovement();
            _sword.XY = new Point(-48, 48);

            RebuildSurface();
        }

        void Step2()
        {
            _animationStep = 2;

            RebuildSurface();

            Timer.Start(this, 500, () =>
            {
                //_surface.FadeOut();
                Timer.Start(this, 700, () =>
                {
                    Stop();
                    return false;
                });
                return false;
            });
        }

        public override bool OnKeyPressed(string key, bool shift, bool control, bool alt)
        {
            if (key == "escape")
                Main.Exit();
            else
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;

                    if (_animationStep <= 1)
                        Step2();
                }
                else if (_animationStep <= 0)
                {
                    Step1();
                    Step2();
                }
            }

            return true;
        }
    }
}
