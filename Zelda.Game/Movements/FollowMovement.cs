﻿using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Movements
{
    class FollowMovement : Movement
    {
        MapEntity _entityFollowed;
        readonly int _x;
        readonly int _y;
        bool _finished;

        public FollowMovement(MapEntity entityFollowed, int x, int y, bool ignoreObstacles)
            : base(ignoreObstacles)
        {
            _entityFollowed = entityFollowed;
            _x = x;
            _y = y;
        }

        public override bool IsFinished
        {
            get { return _finished; }
        }

        public override Point GetDisplayedXY()
        {
            if (_entityFollowed == null)
                return XY;

            // 따라가는 엔티티가 실제 위치와 표현상 위치에 차이가 있다면 그 차이도 적용해줍니다
            Point followedXy = _entityFollowed.XY;
            Point followedDisplayedXy = _entityFollowed.GetDisplayedXY();
            
            Point dxy = followedDisplayedXy - followedXy;
            return XY + dxy;
        }

        public override void Update()
        {
            if (_entityFollowed == null)
                _finished = true;

            if (_entityFollowed.IsBeingRemoved)
            {
                _finished = true;
                _entityFollowed = null;
            }
            else
            {
                int nextX = _entityFollowed.X + _x;
                int nextY = _entityFollowed.Y + _y;

                int dx = nextX - X;
                int dy = nextY - Y;

                if (!IgnoreObstacles)
                {
                    if (!_finished && (dx != 0 || dy != 0))
                    {
                        if (!TestCollisionWithObstacles(dx, dy))
                        {
                            SetX(nextX);
                            SetY(nextY);
                        }
                        else
                        {
                            _finished = true;
                        }
                    }
                }
                else
                {
                    SetX(nextX);
                    SetY(nextY);
                }
            }
        }
    }
}