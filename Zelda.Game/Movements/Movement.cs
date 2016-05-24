using System;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    public abstract class Movement
    {
        public event EventHandler<Point> PositionChanged;

        public bool IgnoreObstacles { get; set; }
        public Point XY => Entity?.XY ?? Drawable?.XY ?? _xy;
        public int X => XY.X;
        public int Y => XY.Y;

        internal MapEntity Entity { get; private set; }
        internal Drawable Drawable { get; private set; }
        internal bool IsSuspended { get; private set; }
        internal bool IsStopped => !IsStarted;
        internal virtual bool IsStarted => false;
        internal virtual bool IsFinished => false;
        internal Rectangle LastCollisionBoxOnObstacle { get; private set; } = new Rectangle(-1, -1);
        internal Action FinishedCallback { get; set; }

        protected int WhenSuspended { get; private set; }

        Point _xy;
        bool _defaultIgnoreObstacles;
        int _lastMoveDate;     // 마지막으로 X나 Y방향으로 이동한 시각
        bool _finished;

        protected Movement(bool ignoreObstacles)
        {
            _defaultIgnoreObstacles = ignoreObstacles;
            IgnoreObstacles = ignoreObstacles;
        }

        internal void SetEntity(MapEntity entity)
        {
            if (Drawable != null)
                throw new InvalidOperationException("This movement is already assigned to a drawable");

            Entity = entity;
            if (Entity == null)
                _xy = new Point(0, 0);
            else
            {
                _xy = Entity.XY;
                NotifyMovementChanged();
            }
            
            NotifyObjectControlled();
        }

        internal void SetDrawable(Drawable drawable)
        {
            if (Entity != null)
                throw new InvalidOperationException("This movement is already assigned to an entity");

            Drawable = drawable;

            if (drawable == null)
                _xy = new Point(0, 0);
            else
            {
                _xy = drawable.XY;
                NotifyMovementChanged();
            }

            NotifyObjectControlled();
        }

        internal virtual void NotifyObjectControlled()
        {
        }

        internal virtual void Update()
        {
            if (!_finished && IsFinished)
            {
                _finished = true;
                NotifyMovementFinished();
            }
            else if (_finished && !IsFinished)
            {
                _finished = false;
            }
        }

        internal virtual void SetSuspended(bool suspended)
        {
            if (suspended == IsSuspended)
                return;

            IsSuspended = suspended;

            if (suspended)
                WhenSuspended = Core.Now;
        }

        public void SetX(int x)
        {
            SetXY(x, Y);
        }

        public void SetY(int y)
        {
            SetXY(X, y);
        }

        public void SetXY(int x, int y)
        {
            SetXY(new Point(x, y));
        }

        public void SetXY(Point xy)
        {
            if (Entity != null)
                Entity.XY = xy;

            if (Drawable != null)
                Drawable.XY = xy;

            _xy = xy;

            NotifyPositionChanged();
            _lastMoveDate = Core.Now;
        }

        internal void TranslateX(int dx)
        {
            TranslateXY(dx, 0);
        }

        internal void TranslateY(int dy)
        {
            TranslateXY(0, dy);
        }

        internal void TranslateXY(int dx, int dy)
        {
            SetXY(X + dx, Y + dy);
        }

        internal void TranslateXY(Point dxy)
        {
            TranslateXY(dxy.X, dxy.Y);
        }

        // X나 Y가 변경되었을 때 호출합니다
        internal virtual void NotifyPositionChanged()
        {
            OnPositionChanged(XY);
            PositionChanged?.Invoke(this, XY);

            if (Entity?.IsBeingRemoved == false)
                Entity.NotifyPositionChanged();
        }

        protected virtual void OnPositionChanged(Point xy) { }

        // 이동 특성(speed이나 angle등)이 변화했음을 알립니다
        internal virtual void NotifyMovementChanged()
        {
            if (Entity?.IsBeingRemoved == false)
                Entity.NotifyMovementChanged();
        }

        // 이동이 끝났을 때 호출합니다
        internal virtual void NotifyMovementFinished()
        {
            FinishedCallback?.Invoke();
            FinishedCallback = null;

            if (Entity?.IsBeingRemoved == false)
                Entity.NotifyMovementFinished();
        }

        internal virtual void Stop()
        {
        }

        internal void SetDefaultIgnoreObstacles(bool ignoreObstacles)
        {
            _defaultIgnoreObstacles = ignoreObstacles;
            IgnoreObstacles = ignoreObstacles;
        }
        
        internal void RestoreDefaultIgnoreObstacles()
        {
            IgnoreObstacles = _defaultIgnoreObstacles;
        }

        internal bool TestCollisionWithObstacles(int dx, int dy)
        {
            if (Entity == null || IgnoreObstacles)
                return false;

            Map map = Entity.Map;

            Rectangle collisionBox = Entity.BoundingBox;
            collisionBox.AddXY(dx, dy);

            bool collision = map.TestCollisionWithObstacles(Entity.Layer, collisionBox, Entity);

            if (collision)
                LastCollisionBoxOnObstacle = collisionBox;

            return collision;
        }

        internal bool TestCollisionWithObstacles(Point dxy)
        {
            return TestCollisionWithObstacles(dxy.X, dxy.Y);
        }

        internal virtual Direction4 GetDisplayedDirection4() => Direction4.Down;    // 기본으로 아래 방향

        internal virtual Point GetDisplayedXY() => XY;
    }
}
