using System;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    public class StraightMovement : Movement
    {
        public bool IsSmooth { get; set; }
        public double Angle { get; private set; }

        // x축 속력 (0:정지, 양수: 우, 음수: 좌)
        internal double XSpeed { get; private set; }

        // y축 속력 (0:정지, 양수: 아래, 음수: 위)
        internal double YSpeed { get; private set; }

        // 초기 위치로부터 이 거리만큼 멀어지거나 장애물과 충돌하면 이동을 멈춥니다
        public int MaxDistance { get; set; }

        internal override bool IsFinished => _finished;
        internal override bool IsStarted => XSpeed != 0 || YSpeed != 0;

        bool _finished;
        int _nextMoveDateX = Core.Now;    // 다음 X 이동 시각
        int _nextMoveDateY = Core.Now;    // 다음 Y 이동 시각
        int _xDelay;           // x축으로의 1픽셀 이동시의 딜레이
        int _yDelay;           // y축으로의 1픽셀 이동시의 딜레이
        int _xMove;             // x축으로의 다음 이동량 (0, 1, -1)
        int _yMove;             // y축으로의 다음 이동량 (0, 1, -1)
        Point _initialXY;       // 초기 위치 (속도나 방향이 변할 때 리셋)

        public StraightMovement(bool ignoreObstacles, bool smooth)
            : base(ignoreObstacles)
        {
            IsSmooth = smooth;
        }

        internal override void Update()
        {
            if (!IsSuspended)
            {
                int now = Core.Now;

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

                    now = Core.Now;

                    if (!_finished && MaxDistance != 0 && Geometry.GetDistance(_initialXY, XY) >= MaxDistance)
                    {
                        SetFinished();
                    }
                    else
                    {
                        xMoveNow = _xMove != 0 && now >= _nextMoveDateX;
                        yMoveNow = _yMove != 0 && now >= _nextMoveDateY;
                    }
                }
            }

            // 기반 클래스가 이동의 끝을 알 수 있도록 함수의 마지막인 여기서 호출해줍니다
            base.Update();
        }

        internal override void NotifyObjectControlled()
        {
            base.NotifyObjectControlled();
            _initialXY = XY;
        }

        internal override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended)
            {
                if (WhenSuspended != 0)
                {
                    int diff = Core.Now - WhenSuspended;
                    _nextMoveDateX += diff;
                    _nextMoveDateY += diff;
                }
            }
        }

        public double GetSpeed()
        {
            return Math.Sqrt(XSpeed * XSpeed + YSpeed * YSpeed);
        }

        public void SetSpeed(double speed)
        {
            if (XSpeed == 0 && YSpeed == 0)
                XSpeed = 1;

            double oldAngle = Angle;
            SetXSpeed(speed * Math.Cos(oldAngle));
            SetYSpeed(-speed * Math.Sin(oldAngle));
            Angle = oldAngle;

            NotifyMovementChanged();
        }

        internal void SetXSpeed(double xSpeed)
        {
            if (Math.Abs(xSpeed) <= 1E-6)
                xSpeed = 0;

            XSpeed = xSpeed;
            int now = Core.Now;

            // _xDelay, _xMove, _nextMoveDateX를 다시 계산합니다
            if (XSpeed == 0)
            {
                _xMove = 0;
            }
            else
            {
                if (xSpeed > 0)
                {
                    _xDelay = (int)(1000 / xSpeed);
                    _xMove = 1;
                }
                else    // xSpeed < 0
                {
                    _xDelay = (int)(1000 / -xSpeed);
                    _xMove = -1;
                }
                SetNextMoveDateX(now + _xDelay);
            }

            Angle = Geometry.GetAngle(0, 0, (int)(xSpeed * 100), (int)(YSpeed * 100));
            _initialXY = XY;
            _finished = false;

            NotifyMovementChanged();
        }

        internal void SetYSpeed(double ySpeed)
        {
            if (Math.Abs(ySpeed) <= 1E-6)
                ySpeed = 0;

            YSpeed = ySpeed;
            int now = Core.Now;

            // _xDelay, _xMove, _nextMoveDateX를 다시 계산합니다
            if (YSpeed == 0)
            {
                _yMove = 0;
            }
            else
            {
                if (ySpeed > 0)
                {
                    _yDelay = (int)(1000 / ySpeed);
                    _yMove = 1;
                }
                else
                {
                    _yDelay = (int)(1000 / -ySpeed);
                    _yMove = -1;
                }
                SetNextMoveDateY(now + _yDelay);
            }

            Angle = Geometry.GetAngle(0, 0, (int)(XSpeed * 100), (int)(ySpeed * 100));
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
            Angle = angle;

            NotifyMovementChanged();
        }

        internal override Direction4 GetDisplayedDirection4()
        {
            int direction = (Geometry.RadiansToDegrees(Angle) + 45 + 360) / 90;
            return (Direction4)(direction % 4);
        }

        internal override void Stop()
        {
            double oldAngle = Angle;
            SetXSpeed(0);
            SetYSpeed(0);
            _xMove = 0;
            _yMove = 0;
            Angle = oldAngle;

            NotifyMovementChanged();
        }

        internal void SetFinished()
        {
            Stop();
            _finished = true;
        }

        protected void SetNextMoveDateX(int nextMoveDateX)
        {
            if (IsSuspended)
            {
                int delay = nextMoveDateX - Core.Now;
                _nextMoveDateX = WhenSuspended + delay;
            }
            else
                _nextMoveDateX = nextMoveDateX;
        }

        protected void SetNextMoveDateY(int nextMoveDateY)
        {
            if (IsSuspended)
            {
                int delay = nextMoveDateY - Core.Now;
                _nextMoveDateY = WhenSuspended + delay;
            }
            else
                _nextMoveDateY = nextMoveDateY;
        }

        protected void UpdateX()
        {
            if (IsSmooth)
                UpdateSmoothX();
            else
                UpdateNonSmoothX();
        }

        protected void UpdateSmoothX()
        {
            if (_xMove == 0)
                return;

            int nextMoveDateXIncrement = _xDelay;

            if (!TestCollisionWithObstacles(_xMove, 0))
            {
                TranslateX(_xMove);
                if (_yMove != 0 && TestCollisionWithObstacles(0, _yMove))
                {
                    // Y 이동이 무효하다면 X 이동으로 속력 전부를 줍니다
                    nextMoveDateXIncrement = (int)(1000.0 / GetSpeed());
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
                        nextMoveDateXIncrement = (int)(_xDelay * Geometry.Sqrt2);  // 속력 보정
                    }
                    else if (!TestCollisionWithObstacles(_xMove, -1) &&
                             (TestCollisionWithObstacles(0, 1) || TestCollisionWithObstacles(0, -1)))
                    {
                        TranslateXY(_xMove, -1);
                        nextMoveDateXIncrement = (int)(_xDelay * Geometry.Sqrt2);
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

        protected void UpdateNonSmoothX()
        {
            if (_xMove == 0)
                return;

            if (!TestCollisionWithObstacles(_xMove, _yMove))
                TranslateX(_xMove);
            else
                Stop();
            
            _nextMoveDateX += _xDelay;
        }

        protected void UpdateY()
        {
            if (IsSmooth)
                UpdateSmoothY();
            else
                UpdateNonSmoothY();
        }

        protected void UpdateSmoothY()
        {
            if (_yMove == 0)
                return;

            int nextMoveDateYIncrement = _yDelay;

            if (!TestCollisionWithObstacles(0, _yMove))
            {
                TranslateY(_yMove);

                if (_xMove != 0 && TestCollisionWithObstacles(_xMove, 0))
                    nextMoveDateYIncrement = (int)(1000.0 / GetSpeed());
            }
            else
            {
                if (_xMove == 0)
                {
                    if (!TestCollisionWithObstacles(1, _yMove) &&
                        (TestCollisionWithObstacles(-1, 0) || TestCollisionWithObstacles(1, 0)))
                    {
                        TranslateXY(1, _yMove);
                        nextMoveDateYIncrement = (int)(_yDelay * Geometry.Sqrt2);
                    }
                    else if (!TestCollisionWithObstacles(-1, _yMove) &&
                             (TestCollisionWithObstacles(1, 0) || TestCollisionWithObstacles(-1, 0)))
                    {
                        TranslateXY(-1, _yMove);
                        nextMoveDateYIncrement = (int)(_yDelay * Geometry.Sqrt2);
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

        protected void UpdateNonSmoothY()
        {
            if (_yMove == 0)
                return;

            if (!TestCollisionWithObstacles(_xMove, _yMove))
                TranslateY(_yMove);
            else
                Stop();
            _nextMoveDateY += _yDelay;
        }
    }
}
