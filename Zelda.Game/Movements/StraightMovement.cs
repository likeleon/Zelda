using System;
using System.ComponentModel;
using Zelda.Game.Engine;

namespace Zelda.Game.Movements
{
    class StraightMovement : Movement
    {
        uint _nextMoveDateX = EngineSystem.Now;    // 다음 X 이동 시각
        uint _nextMoveDateY = EngineSystem.Now;    // 다음 Y 이동 시각
        uint _xDelay;           // x축으로의 1픽셀 이동시의 딜레이
        uint _yDelay;           // y축으로의 1픽셀 이동시의 딜레이
        int _xMove;             // x축으로의 다음 이동량 (0, 1, -1)
        int _yMove;             // y축으로의 다음 이동량 (0, 1, -1)
        Point _initialXY;       // 초기 위치 (속도나 방향이 변할 때 리셋)

        public StraightMovement(bool ignoreObstacles)
            : base(ignoreObstacles)
        {
        }

        public override void Update()
        {
            uint now = EngineSystem.Now;

            bool xMoveNow = _xMove != 0 && now >= _nextMoveDateX;
            bool yMoveNow = _yMove != 0 && now >= _nextMoveDateY;

            while (xMoveNow || yMoveNow)
            {
                Point oldXY = XY;

                if (xMoveNow)
                {
                    if (yMoveNow)
                    {
                        // x와 y 모두 이동시켜야 합니다
                        if (_nextMoveDateX <= _nextMoveDateY)
                        {
                            // x를 먼저 이동시킵니다
                            UpdateX();
                            if (now >= _nextMoveDateY)
                                UpdateY();
                        }
                        else
                        {
                            // y를 먼저 이동시킵니다
                            UpdateY();
                            if (now >= _nextMoveDateX)
                                UpdateX();
                        }
                    }
                    else
                    {
                        UpdateX();
                    }
                }
                else
                {
                    UpdateY();
                }

                now = EngineSystem.Now;

                if (!_finished && _maxDistance != 0 && Geometry.GetDistance(_initialXY, XY) >= _maxDistance)
                {
                    SetFinished();
                }
                else
                {
                    xMoveNow = _xMove != 0 && now >= _nextMoveDateX;
                    yMoveNow = _yMove != 0 && now >= _nextMoveDateY;
                }
            }

            // Movement 클래스가 종료를 알 수 있도록 마지막에 호출해줘야합니다
            base.Update();
        }

        public override void NotifyObjectControlled()
        {
            base.NotifyObjectControlled();
            _initialXY = XY;
        }

        #region 스피드 벡터
        double _angle;
        public double Angle
        {
            get { return _angle; }
        }

        // x축 속력 (0:정지, 양수: 우, 음수: 좌)
        double _xSpeed;
        public double XSpeed
        {
            get { return _xSpeed; }
        }

        // y축 속력 (0:정지, 양수: 아래, 음수: 위)
        double _ySpeed;
        public double YSpeed
        {
            get { return _ySpeed; }
        }

        // 초기 위치로부터 이 거리만큼 멀어지거나 장애물과 충돌하면 이동을 멈춥니다
        int _maxDistance;
        public int MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }
        public double GetSpeed()
        {
            return Math.Sqrt(_xSpeed * _xSpeed + _ySpeed * _ySpeed);
        }

        public void SetSpeed(double speed)
        {
            if (_xSpeed == 0 && _ySpeed == 0)
                _xSpeed = 1;

            double oldAngle = _angle;
            SetXSpeed(speed * Math.Cos(oldAngle));
            SetYSpeed(-speed * Math.Sin(oldAngle));
            _angle = oldAngle;

            NotifyMovementChanged();
        }

        public void SetXSpeed(double xSpeed)
        {
            if (Math.Abs(xSpeed) <= 1E-6)
                xSpeed = 0;

            _xSpeed = xSpeed;
            uint now = EngineSystem.Now;

            // _xDelay, _xMove, _nextMoveDateX를 다시 계산합니다
            if (_xSpeed == 0)
            {
                _xMove = 0;
            }
            else
            {
                if (xSpeed > 0)
                {
                    _xDelay = (uint)(1000 / xSpeed);
                    _xMove = 1;
                }
                else    // xSpeed < 0
                {
                    _xDelay = (uint)(1000 / -xSpeed);
                    _xMove = -1;
                }
                SetNextMoveDateX(now + _xDelay);
            }

            _angle = Geometry.GetAngle(0, 0, (int)(xSpeed * 100), (int)(_ySpeed * 100));
            _initialXY = XY;
            _finished = false;

            NotifyMovementChanged();
        }

        public void SetYSpeed(double ySpeed)
        {
            if (Math.Abs(ySpeed) <= 1E-6)
                ySpeed = 0;

            _ySpeed = ySpeed;
            uint now = EngineSystem.Now;

            // _xDelay, _xMove, _nextMoveDateX를 다시 계산합니다
            if (_ySpeed == 0)
            {
                _yMove = 0;
            }
            else
            {
                if (ySpeed > 0)
                {
                    _yDelay = (uint)(1000 / ySpeed);
                    _yMove = 1;
                }
                else
                {
                    _yDelay = (uint)(1000 / -ySpeed);
                    _yMove = -1;
                }
                SetNextMoveDateY(now + _yDelay);
            }

            _angle = Geometry.GetAngle(0, 0, (int)(_xSpeed * 100), (int)(ySpeed * 100));
            _initialXY = XY;
            _finished = false;

            NotifyMovementChanged();
        }

        public void SetAngle(double angle)
        {
            if (!IsStopped)
            {
                double speed = GetSpeed();
                SetXSpeed(speed * Math.Cos(angle));
                SetYSpeed(-speed * Math.Sin(angle));
            }
            _angle = angle;

            NotifyMovementChanged();
        }
        #endregion

        #region 이동
        bool _finished;
        public override bool IsFinished
        {
            get { return _finished; }
        }

        public override bool IsStarted
        {
            get { return _xSpeed != 0 || _ySpeed != 0; }
        }

        public override void Stop()
        {
            double oldAngle = _angle;
            SetXSpeed(0);
            SetYSpeed(0);
            _xMove = 0;
            _yMove = 0;
            _angle = oldAngle;

            NotifyMovementChanged();
        }

        public void SetFinished()
        {
            Stop();
            _finished = true;
        }
        #endregion

        protected void SetNextMoveDateX(uint nextMoveDateX)
        {
            _nextMoveDateX = nextMoveDateX;
        }

        protected void SetNextMoveDateY(uint nextMoveDateY)
        {
            _nextMoveDateY = nextMoveDateY;
        }

        protected void UpdateX()
        {
            UpdateSmoothX();
        }

        protected void UpdateSmoothX()
        {
            if (_xMove != 0)    // x 방향으로의 이동이 필요
            {
                // TODO: 기본적으로 _nextMoveDateX는 _xDelay만큼 증가하지만
                // x 속도의 수정이 필요하다면 _nextMoveDateX를 수정합니다
                uint nextMoveDateX = _xDelay;

                TranslateX(_xMove); // 이동시킵니다

                _nextMoveDateX += nextMoveDateX;
            }
        }

        protected void UpdateY()
        {
            UpdateSmoothY();
        }

        protected void UpdateSmoothY()
        {
            if (_yMove != 0)    // y 방향으로의 이동이 필요
            {
                // TODO: 기본적으로 _nextMoveDateX는 _yDelay만큼 증가하지만
                // y 속도의 수정이 필요하다면 _nextMoveDatey를 수정합니다
                uint nextMoveDateY = _yDelay;

                TranslateY(_yMove); // 이동시킵니다

                _nextMoveDateY += nextMoveDateY;
            }
        }
    }
}
