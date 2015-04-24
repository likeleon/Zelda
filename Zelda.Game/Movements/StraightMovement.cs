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
            if (_xMove == 0)
                return;

            uint nextMoveDateXIncrement = _xDelay;

            if (!TestCollisionWithObstacles(_xMove, 0))
            {
                TranslateX(_xMove);
                if (_yMove != 0 && TestCollisionWithObstacles(0, _yMove))
                {
                    // Y 이동이 무효하다면 X 이동으로 속력 전부를 줍니다
                    nextMoveDateXIncrement = (uint)(1000.0 / GetSpeed());
                }
            }
            else
            {
                if (_yMove == 0)
                {
                    // X 이동이 불가능하고 Y 이동이 없는 경우 Y 방향으로 대각선 이동을 시도합니다
                    if (!TestCollisionWithObstacles(_xMove, 1) &&
                        (TestCollisionWithObstacles(0, -1) || TestCollisionWithObstacles(0, 1)))
                    {
                        TranslateXY(_xMove, 1);
                        nextMoveDateXIncrement = (uint)(_xDelay * Geometry.Sqrt2);  // 속력 보정
                    }
                    else if (!TestCollisionWithObstacles(_xMove, -1) &&
                             (TestCollisionWithObstacles(0, 1) || TestCollisionWithObstacles(0, -1)))
                    {
                        TranslateXY(_xMove, -1);
                        nextMoveDateXIncrement = (uint)(_xDelay * Geometry.Sqrt2);
                    }
                    else
                    {
                        // 대각선 이동도 불가능합니다. 위/아래 8픽셀만큼 이동가능한 위치를 찾고, 해당 위치로 이동합니다
                        bool moved = false;
                        for (int i = 1; i <= 8 && !moved; ++i)
                        {
                            if (!TestCollisionWithObstacles(_xMove, i) && !TestCollisionWithObstacles(0, 1))
                            {
                                TranslateY(1);
                                moved = true;
                            }
                            else if (!TestCollisionWithObstacles(_xMove, -i) && !TestCollisionWithObstacles(0, -1))
                            {
                                TranslateY(-1);
                                moved = true;
                            }
                        }
                    }
                }
                else
                {
                    // X 이동은 불가능하지만 Y 이동량이 존재합니다
                    if (!TestCollisionWithObstacles(0, _yMove))
                    {
                        // 불필요하게 기다리지 않고, Y 이동을 바로 수행합니다.
                        UpdateY();
                    }
                    else
                    {
                        // X, Y 모두 이동이 불가능합니다
                        // 매우 좁은 대각선 방향의 길에서 필요하므로 X와 Y 양쪽으로 한번에 이동하는 것을 시도해봅니다.
                        // 이는 가능한 마지막 해결책이어야 하고, 센서를 스킵해서 지나갈 수 있기 때문에 X와 Y 각 방향으로 이동하는 것이 기본입니다.
                        if (!TestCollisionWithObstacles(_xMove, _yMove))
                        {
                            TranslateXY(_xMove, _yMove);
                            _nextMoveDateY += _yDelay;  // Y를 수정했기 때문에 Y 이동 갱신 시점을 업데이트해주어야 합니다.
                        }
                    }
                }
            }

            _nextMoveDateX += nextMoveDateXIncrement;
        }

        protected void UpdateY()
        {
            UpdateSmoothY();
        }

        protected void UpdateSmoothY()
        {
            if (_yMove == 0)
                return;

            uint nextMoveDateYIncrement = _yDelay;

            if (!TestCollisionWithObstacles(0, _yMove))
            {
                TranslateY(_yMove);

                if (_xMove != 0 && TestCollisionWithObstacles(_xMove, 0))
                    nextMoveDateYIncrement = (uint)(1000.0 / GetSpeed());
            }
            else
            {
                if (_xMove == 0)
                {
                    if (!TestCollisionWithObstacles(1, _yMove) &&
                        (TestCollisionWithObstacles(-1, 0) || TestCollisionWithObstacles(1, 0)))
                    {
                        TranslateXY(1, _yMove);
                        nextMoveDateYIncrement = (uint)(_yDelay * Geometry.Sqrt2);
                    }
                    else if (!TestCollisionWithObstacles(-1, _yMove) &&
                             (TestCollisionWithObstacles(1, 0) || TestCollisionWithObstacles(-1, 0)))
                    {
                        TranslateXY(-1, _yMove);
                        nextMoveDateYIncrement = (uint)(_yDelay * Geometry.Sqrt2);
                    }
                    else
                    {
                        bool moved = false;
                        for (int i = 1; i <= 8 && !moved; ++i)
                        {
                            if (!TestCollisionWithObstacles(i, _yMove) && !TestCollisionWithObstacles(1, 0))
                            {
                                TranslateX(1);
                                moved = true;
                            }
                            else if (!TestCollisionWithObstacles(-i, _yMove) && !TestCollisionWithObstacles(-1, 0))
                            {
                                TranslateX(-1);
                                moved = true;
                            }
                        }
                    }
                }
                else
                {
                    if (!TestCollisionWithObstacles(_xMove, 0))
                        UpdateX();
                    else
                    {
                        if (!TestCollisionWithObstacles(_xMove, _yMove))
                        {
                            TranslateXY(_xMove, _yMove);
                            _nextMoveDateX += _xDelay;
                        }
                    }
                }
            }

            _nextMoveDateY += nextMoveDateYIncrement;
        }
    }
}
