using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Engine;

namespace Zelda.Game.Movements
{
    class TargetMovement : StraightMovement
    {
        Point _target;
        int _movingSpeed;
        uint _nextRecomputationDate;
        bool _finished;
        int _signX;         // X 방향 (1: 우, -1: 좌)
        int _signY;         // Y 방향 (1: 아래, -1: 위)

        static readonly uint s_recomputationDelay = 150;

        public TargetMovement(Point target, int movingSpeed)
        {
            _target = target;
            _signX = 0;
            _signY = 0;
            _movingSpeed = movingSpeed;
            _nextRecomputationDate = EngineSystem.Now;
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
            _nextRecomputationDate = EngineSystem.Now + s_recomputationDelay;
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
    }
}
