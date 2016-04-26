using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    class PathMovement : PixelMovement
    {
        public PathMovement(List<Direction8> path, int speed, bool loop, bool ignoreObstacles, bool snapToGrid)
            : base("", 0, false, ignoreObstacles)
        {
            CurrentDirection = Direction8.Down;
            Speed = speed;
            _loop = loop;
            SnapToGrid = snapToGrid;
            SetPath(path);
        }

        public Direction8 CurrentDirection { get; private set; }
        public int TotalDistanceCovered { get; private set; }
        public int Speed { get; set; }

        bool _loop;
        public new bool Loop
        {
            get { return _loop; }
        }

        public bool SnapToGrid { get; set; }

        List<Direction8> _initialPath;
        public List<Direction8> Path
        {
            get { return _initialPath; }
        }

        List<Direction8> _remainingPath;
        bool _snapping;
        uint _stopSnappingDate;
        bool _stoppedByObstacle;

        readonly static string[] _elementaryMoves = new string[]
        {
            " 1  0   1  0   1  0   1  0   1  0   1  0   1  0   1  0", // 8 pixels right
            " 1 -1   1 -1   1 -1   1 -1   1 -1   1 -1   1 -1   1 -1", // 8 pixels right-up
            " 0 -1   0 -1   0 -1   0 -1   0 -1   0 -1   0 -1   0 -1", // 8 pixels up
            "-1 -1  -1 -1  -1 -1  -1 -1  -1 -1  -1 -1  -1 -1  -1 -1", // 8 pixels left-up
            "-1  0  -1  0  -1  0  -1  0  -1  0  -1  0  -1  0  -1  0", // 8 pixels left
            "-1  1  -1  1  -1  1  -1  1  -1  1  -1  1  -1  1  -1  1", // 8 pixels left-down
            " 0  1   0  1   0  1   0  1   0  1   0  1   0  1   0  1", // 8 pixels down
            " 1  1   1  1   1  1   1  1   1  1   1  1   1  1   1  1"  // 8 pixels right-down
        };

        readonly static Direction4[] _displayedDirections = new Direction4[] 
        { 
            Direction4.Right,
            Direction4.Right,
            Direction4.Up,
            Direction4.Left,
            Direction4.Left,
            Direction4.Left,
            Direction4.Down,
            Direction4.Right
        };

        public override Direction4 GetDisplayedDirection4()
        {
            return _displayedDirections[(int)CurrentDirection];
        }

        public new void SetLoop(bool loop)
        {
            _loop = loop;

            if (base.IsFinished && _remainingPath.Count == 0 && _loop)
                Restart();
        }

        public void SetPath(List<Direction8> path)
        {
            _initialPath = path;
            Restart();
        }

        void Restart()
        {
            _remainingPath = _initialPath;
            _snapping = false;
            _stopSnappingDate = 0;
            _stoppedByObstacle = false;

            StartNextElementaryMove();
        }

        void StartNextElementaryMove()
        {
            if (Entity == null)
                return;

            // 이동 전에 엔티티가 8x8영역에 맞춰져 있는지 확인합니다 (필요할 경우)
            if (SnapToGrid && !Entity.IsAlignedToGrid)
                Snap();

            if (!SnapToGrid || Entity.IsAlignedToGrid)
            {
                _snapping = false;

                if (_remainingPath.Count <= 0)
                {
                    if (_loop)
                        _remainingPath = _initialPath;
                    else if (!IsStopped)
                        Stop();
                }

                if (_remainingPath.Count > 0)
                {
                    CurrentDirection = _remainingPath[0];
                    Debug.CheckAssertion((int)CurrentDirection >= 0 && (int)CurrentDirection < 8,
                        "Invalid path '{0}' (bad direction '{1}'".F(String.Join(" ", _initialPath), _remainingPath[0]));

                    Delay = SpeedToDelay(Speed, CurrentDirection);
                    SetTrajectory(_elementaryMoves[(int)CurrentDirection]);
                    _remainingPath.RemoveAt(0);
                }
            }
        }

        // 속력을 각 픽셀 이동 사이의 지연 시간(밀리초)로 변환합니다
        static uint SpeedToDelay(int speed, Direction8 direction)
        {
            uint delay = (uint)(1000 / speed);  // 초당 속력 (in 픽셀)
            if ((int)direction % 2 != 0)
                delay = (uint)(delay * Geometry.Sqrt2);
            return delay;
        }

        void Snap()
        {
            // 엔티티의 좌상단 위치에서 가장 가까운 그리드 교점의 좌표를 찾습니다
            int x = Entity.TopLeftX;
            int y = Entity.TopLeftY;
            int snappedX = x + 4;
            int snappedY = y + 4;
            snappedX -= snappedX % 8;
            snappedY -= snappedY % 8;

            uint now = Framework.Now;

            if (!_snapping)
            {
                SetSnappingTrajectory(new Point(x, y), new Point(snappedX, snappedY));
                _snapping = true;
                _stopSnappingDate = now + 500;
            }
            else
            {
                if (now >= _stopSnappingDate)
                {
                    // 타임아웃때까지 스냅시키지 못했다면 반대 방향으로 시도합니다
                    snappedX += (snappedX < x) ? 8 : -8;
                    snappedY += (snappedY < y) ? 8 : -8;
                    SetSnappingTrajectory(new Point(x, y), new Point(snappedX, snappedY));
                    _stopSnappingDate = now + 500;
                }
            }
        }

        void SetSnappingTrajectory(Point src, Point dst)
        {
            List<Point> trajectory = new List<Point>();
            Point xy = src;
            while (xy != dst)
            {
                int dx = dst.X - xy.X;
                int dy = dst.Y - xy.Y;

                if (dx > 0)
                    dx = 1;
                else if (dx < 0)
                    dx = -1;

                if (dy > 0)
                    dy = 1;
                else if (dy < 0)
                    dy = -1;

                trajectory.Add(new Point(dx, dy));
                xy += new Point(dx, dy);
            }
            Delay = SpeedToDelay(Speed, Direction8.Right); // 대각선 처리 제외
            base.SetLoop(false);
            SetTrajectory(trajectory);
        }

        public override void NotifyObjectControlled()
        {
            base.NotifyObjectControlled();
            Restart();
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended &&
                WhenSuspended != 0 &&
                _stopSnappingDate != 0)
            {
                _stopSnappingDate += Framework.Now -WhenSuspended;
            }
        }

        public override bool IsFinished
        {
            get
            {
                return (base.IsFinished && _remainingPath.Count <= 0 && !Loop) ||
                    _stoppedByObstacle;
            }
        }

        public override void Update()
        {
            while (!IsSuspended &&
                   IsCurrentElementaryMoveFinished &&
                   !IsFinished &&
                   Entity != null)
            {
                StartNextElementaryMove();
                base.Update();
            }

            base.Update();
        }

        bool IsCurrentElementaryMoveFinished
        {
            get { return base.IsFinished; }
        }

        protected override void NotifyStepDone(int stepIndex, bool success)
        {
            if (success)
            {
                if (!_snapping)
                    ++TotalDistanceCovered;
            }
            else
                _stoppedByObstacle = true;
        }
    }
}
