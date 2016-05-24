using System;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    public class TargetMovement : StraightMovement
    {
        internal override bool IsFinished => _finished;

        static readonly int RecomputationDelay = 150;

        bool _finished;
        Point _target;
        int _movingSpeed;
        int _nextRecomputationDate;
        int _signX;         // X 방향 (1: 우, -1: 좌)
        int _signY;         // Y 방향 (1: 아래, -1: 위)

        public static TargetMovement Create()
        {
            if (Core.Game != null)
                throw new NotImplementedException("If we are on a map, the default target should be the hero.");
            else
                return new TargetMovement(Point.Zero, 32, false);
        }

        internal TargetMovement(Point target, int movingSpeed, bool ignoreObstacles)
            : base(ignoreObstacles, true)
        {
            _target = target;
            _signX = 0;
            _signY = 0;
            _movingSpeed = movingSpeed;
            _nextRecomputationDate = Core.Now;
        }

        public void Start(Point xy, Action finished)
        {
            Stop();
            SetXY(xy);
            FinishedCallback = finished;
        }

        public void Start(Drawable drawable, Action finished)
        {
            Stop();
            drawable.StartMovement(this);
            FinishedCallback = finished;
        }

        public void Start(MapEntity entity, Action finished)
        {
            Stop();
            entity.ClearMovement();
            entity.SetMovement(this);
            FinishedCallback = finished;
        }

        public void SetMovingSpeed(int movingSpeed)
        {
            _movingSpeed = movingSpeed;
            RecomputeMovement();
        }

        public void SetTarget(Point xy)
        {
            _target = xy;

            RecomputeMovement();
            _nextRecomputationDate = Core.Now + RecomputationDelay;
        }

        // 타겟에 기반해 방향과 속력을 계산합니다
        void RecomputeMovement()
        {
            if (XY != _target)
            {
                _finished = false;

                double angle = Geometry.GetAngle(XY, _target);

                Point dxy = _target - XY;
                _signX = (dxy.X >= 0) ? 1 : -1;
                _signY = (dxy.Y >= 0) ? 1 : -1;

                if (Math.Abs(angle - Angle) > 1E-6 || GetSpeed() < 1E-6)
                {
                    // 각도가 변했거나 이동이 멈춘 경우
                    SetSpeed(_movingSpeed);
                    SetAngle(angle);
                    MaxDistance = (int)Geometry.GetDistance(XY, _target);
                }
            }
        }

        internal override void NotifyObjectControlled()
        {
            base.NotifyObjectControlled();

            // 좌표가 변경되었으니 다시 계산합니다
            RecomputeMovement();
        }

        internal override void Update()
        {
            if (Core.Now >= _nextRecomputationDate)
            {
                RecomputeMovement();
                _nextRecomputationDate += RecomputationDelay;
            }

            // 타겟에 다다랐는지를 확인합니다
            Point dxy = _target - XY;
            if (dxy.X * _signX <= 0 && dxy.Y * _signY <= 0)
            {
                if (!TestCollisionWithObstacles(dxy))
                {
                    SetXY(_target); // 딱 맞게 이동하지 않을 가능성이 있기 때문에 명시적으로 설정
                    Stop();
                    _finished = true;
                }
            }

            base.Update();
        }
    }
}
