using Zelda.Game.Entities;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    public class FollowMovement : Movement
    {
        internal override bool IsFinished => _finished;

        Entity _entityFollowed;
        readonly int _x;
        readonly int _y;
        bool _finished;

        public FollowMovement(Entity entityFollowed, int x, int y, bool ignoreObstacles)
            : base(ignoreObstacles)
        {
            _entityFollowed = entityFollowed;
            _x = x;
            _y = y;
        }


        internal override Point GetDisplayedXY()
        {
            if (_entityFollowed == null)
                return XY;

            // 따라가는 엔티티가 실제 위치와 표현상 위치에 차이가 있다면 그 차이도 적용해줍니다
            Point followedXy = _entityFollowed.XY;
            Point followedDisplayedXy = _entityFollowed.GetDisplayedXY();
            
            Point dxy = followedDisplayedXy - followedXy;
            return XY + dxy;
        }

        internal override void Update()
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
