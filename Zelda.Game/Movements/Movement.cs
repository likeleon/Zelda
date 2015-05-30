using System;
using Zelda.Game.Engine;
using Zelda.Game.Entities;
using Zelda.Game.Script;

namespace Zelda.Game.Movements
{
    abstract class Movement
    {
        protected Movement(bool ignoreObstacles)
        {
            _defaultIgnoreObstacles = ignoreObstacles;
            _currentIgnoreObstacles = ignoreObstacles;
        }

        #region 조종 오브젝트
        MapEntity _entity;

        public MapEntity Entity
        {
            get { return _entity; }
        }

        public void SetEntity(MapEntity entity)
        {
            Debug.CheckAssertion(_drawable == null, "This movement is already assigned to a drawable");

            _entity = entity;
            if (_entity == null)
                _xy = new Point(0, 0);
            else
            {
                _xy = _entity.XY;
                NotifyMovementChanged();
            }
            
            NotifyObjectControlled();
        }

        Drawable _drawable;
        public Drawable Drawable
        {
            get { return _drawable; }
        }

        public void SetDrawable(Drawable drawable)
        {

            Debug.CheckAssertion(_entity == null, "This movement is already assigned to an entity");

            _drawable = drawable;

            if (drawable == null)
                _xy = new Point(0, 0);
            else
            {
                _xy = drawable.XY;
                NotifyMovementChanged();
            }

            NotifyObjectControlled();
        }

        public virtual void NotifyObjectControlled()
        {
        }
        #endregion

        #region 갱신
        uint _lastMoveDate;     // 마지막으로 X나 Y방향으로 이동한 시각
        bool _finished;

        public virtual void Update()
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

        bool _suspended;
        public bool IsSuspended
        {
            get { return _suspended; }
        }

        uint _whenSuspended;
        protected uint WhenSuspended
        {
            get { return _whenSuspended; }
        }

        public virtual void SetSuspended(bool suspended)
        {
            if (suspended == _suspended)
                return;

            _suspended = suspended;

            if (suspended)
                _whenSuspended = EngineSystem.Now;
        }
        #endregion

        #region 위치
        Point _xy;
        public Point XY
        {
            get
            {
                if (_entity != null)
                    return _entity.XY;

                if (_drawable != null)
                    return _drawable.XY;

                return _xy; 
            }
        }

        public int X
        {
            get { return XY.X; }
        }

        public void SetX(int x)
        {
            SetXY(x, Y);
        }

        public int Y
        {
            get { return XY.Y; }
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
            if (_entity != null)
                _entity.XY = xy;

            if (_drawable != null)
                _drawable.XY = xy;

            _xy = xy;

            NotifyPositionChanged();
            _lastMoveDate = EngineSystem.Now;
        }

        public void TranslateX(int dx)
        {
            TranslateXY(dx, 0);
        }

        public void TranslateY(int dy)
        {
            TranslateXY(0, dy);
        }

        public void TranslateXY(int dx, int dy)
        {
            SetXY(X + dx, Y + dy);
        }

        public void TranslateXY(Point dxy)
        {
            TranslateXY(dxy.X, dxy.Y);
        }

        // X나 Y가 변경되었을 때 호출합니다
        public virtual void NotifyPositionChanged()
        {
            if (_scriptMovement != null)
                _scriptMovement.NotifyPositionChanged(XY);

            if (_entity != null && !_entity.IsBeingRemoved)
                _entity.NotifyPositionChanged();
        }

        // 이동 특성(speed이나 angle등)이 변화했음을 알립니다
        public virtual void NotifyMovementChanged()
        {
            if (_entity != null && !_entity.IsBeingRemoved)
                _entity.NotifyMovementChanged();
        }

        // 이동이 끝났을 때 호출합니다
        public virtual void NotifyMovementFinished()
        {
            if (_scriptMovement != null)
            {
                if (_finishedCallback != null)
                {
                    _finishedCallback.Invoke();
                    _finishedCallback = null;
                }
                _scriptMovement.NotifyMovementFinished();
            }

            if (_entity != null && !_entity.IsBeingRemoved)
                _entity.NotifyMovementFinished();
        }
        #endregion

        #region 이동
        public bool IsStopped
        {
            get { return !IsStarted; }
        }

        public virtual bool IsStarted
        {
            get { return false; }
        }

        public virtual void Stop()
        {
        }

        public virtual bool IsFinished
        {
            get { return false; }
        }
        #endregion

        #region 충돌
        bool _defaultIgnoreObstacles;
        
        bool _currentIgnoreObstacles;
        public bool IgnoreObstacles
        {
            get { return _currentIgnoreObstacles; }
            set { _currentIgnoreObstacles = value; }
        }

        public void SetDefaultIgnoreObstacles(bool ignoreObstacles)
        {
            _defaultIgnoreObstacles = ignoreObstacles;
            _currentIgnoreObstacles = ignoreObstacles;
        }
        
        public void RestoreDefaultIgnoreObstacles()
        {
            _currentIgnoreObstacles = _defaultIgnoreObstacles;
        }

        Rectangle _lastCollisionBoxOnObstacle = new Rectangle(-1, -1);
        public Rectangle LastCollisionBoxOnObstacle
        {
            get { return _lastCollisionBoxOnObstacle; }
        }

        public bool TestCollisionWithObstacles(int dx, int dy)
        {
            if (_entity == null || _currentIgnoreObstacles)
                return false;

            Map map = _entity.Map;

            Rectangle collisionBox = _entity.BoundingBox;
            collisionBox.AddXY(dx, dy);

            bool collision = map.TestCollisionWithObstacles(_entity.Layer, collisionBox, _entity);

            if (collision)
                _lastCollisionBoxOnObstacle = collisionBox;

            return collision;
        }

        public bool TestCollisionWithObstacles(Point dxy)
        {
            return TestCollisionWithObstacles(dxy.X, dxy.Y);
        }
        #endregion

        #region 이동 오브젝트 표시
        public virtual Direction4 GetDisplayedDirection4()
        {
            return Direction4.Down;   // 기본으로 아래 방향
        }

        public virtual Point GetDisplayedXY()
        {
            return XY;
        }
        #endregion

        #region 스크립트
        ScriptMovement _scriptMovement;
        public ScriptMovement ScriptMovement
        {
            get { return _scriptMovement; }
            set { _scriptMovement = value; }
        }

        Action _finishedCallback;
        public Action FinishedCallback
        {
            get { return _finishedCallback; }
            set
            {
                Debug.CheckAssertion(ScriptMovement != null, "Undefined ScriptMovement");

                _finishedCallback = value;
            }
        }
        #endregion
    }
}
