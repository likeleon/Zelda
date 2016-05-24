using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    class PlayerMovement : StraightMovement
    {
        internal int MovingSpeed { get; }
        internal Direction8 WantedDirection8 { get; private set; }

        public PlayerMovement(int movingSpeed)
            : base(false, true)
        {
            MovingSpeed = movingSpeed;
            WantedDirection8 = Direction8.None;
        }

        internal override void Update()
        {
            base.Update();

            if (IsSuspended)
                return;

            if (Entity?.IsOnMap == false)
                return;

            if (IsStopped && WantedDirection8 != Direction8.None)
            {
                // Hero.ResetMovement()등에 의해 이동이 멈춘 경우
                WantedDirection8 = Direction8.None;
                ComputeMovement();
            }
            else
            {
                // 의도한 방향이 변경되었는지 확인합니다
                var wantedDirection8 = Entity.Game.Commands.GetWantedDirection8();
                if (wantedDirection8 != WantedDirection8)
                {
                    WantedDirection8 = wantedDirection8;
                    ComputeMovement();
                }
            }
        }

        internal void ComputeMovement()
        {
            if (WantedDirection8 == Direction8.None)
            {
                // 이동 없음
                Stop();
            }
            else
            {
                SetSpeed(MovingSpeed);
                SetAngle(Geometry.DegreesToRadians((int)WantedDirection8 * 45));
            }

            NotifyMovementChanged();
        }
    }
}
