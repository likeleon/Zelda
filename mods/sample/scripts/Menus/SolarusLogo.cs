using System;
using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Sample.Menus
{
    class SolarusLogo : ScriptMenu
    {
        readonly ScriptSurface _surface;
        readonly ScriptSprite _title;
        readonly ScriptSprite _subtitle;
        readonly ScriptSprite _sun;
        readonly ScriptSprite _sword;
        readonly ScriptSurface _blackSquare;
        int _animationStep;
        Timer _timer;

        public SolarusLogo()
        {
            _surface = ScriptSurface.Create(201, 48);
            
            _title = ScriptSprite.Create("menus/solarus_logo");
            _title.SetAnimation("title");

            _subtitle = ScriptSprite.Create("menus/solarus_logo");
            _subtitle.SetAnimation("subtitle");

            _sun = ScriptSprite.Create("menus/solarus_logo");
            _sun.SetAnimation("sun");

            _sword = ScriptSprite.Create("menus/solarus_logo");
            _sword.SetAnimation("sword");

            _blackSquare = ScriptSurface.Create(48, 15);
            _blackSquare.FillColor(Color.Black);
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
            _surface.SetOpacity(255);
            _sun.SetDirection(0);
            _sun.XY = new Point(0, 0);
            _sword.XY = new Point(0, 0);

            StartAnimation();
            RebuildSurface();
        }

        void StartAnimation()
        {
            // 태양의 이동
            ScriptTargetMovement sunMovement = ScriptMovement.Create(MovementType.Target) as ScriptTargetMovement;
            sunMovement.SetSpeed(64);
            sunMovement.SetTarget(new Point(0, -33));
            sunMovement.PositionChanged += (o, e) => RebuildSurface();

            // 검의 이동
            ScriptTargetMovement swordMovement = ScriptMovement.Create(MovementType.Target) as ScriptTargetMovement;
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
                        });
                    }
                });
            });
        }

        void Step1()
        {
            _animationStep = 1;
            
            _sun.SetDirection(Direction4.Up);
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
                _surface.FadeOut(null, null);
                Timer.Start(this, 700, (Action)Stop);
            });
        }
        
        protected override void OnDraw(ScriptSurface screen)
        {
            _surface.Draw(screen, screen.Width / 2 - 100, screen.Height / 2 - 24);
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            if (key == KeyboardKey.Escape)
                ScriptMain.Exit();
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
