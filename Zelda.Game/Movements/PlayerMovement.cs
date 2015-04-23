using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Movements
{
    class PlayerMovement : StraightMovement
    {
        readonly int _movingSpeed;
        public int MovingSpeed
        {
            get { return _movingSpeed; }
        }
        
        int _direction8;
        public int WantedDirection8
        {
            get { return _direction8; }
        }

        public PlayerMovement(int movingSpeed)
            : base(false)
        {
            _movingSpeed = movingSpeed;
            _direction8 = -1;
        }

        public override void Update()
        {
            base.Update();

            MapEntity entity = Entity;
            if (entity == null || !entity.IsOnMap)
                return;

            if (IsStopped && _direction8 != -1)
            {
                // Hero.ResetMovement()등에 의해 이동이 멈춘 경우
                _direction8 = -1;
                ComputeMovement();
            }
            else
            {
                // 의도한 방향이 변경되었는지 확인합니다
                GameCommands commands = Entity.Game.Commands;
                int wantedDirection8 = commands.GetWantedDirection8();
                if (wantedDirection8 != _direction8)
                {
                    _direction8 = wantedDirection8;
                    ComputeMovement();
                }
            }
        }

        public void ComputeMovement()
        {
            if (_direction8 == -1)
            {
                // 이동 없음
                Stop();
            }
            else
            {
                SetSpeed(_movingSpeed);
                SetAngle(Geometry.DegreesToRadians(_direction8 * 45));
            }

            NotifyMovementChanged();
        }
    }
}
