using System;
using System.ComponentModel;
using Zelda.Game.Engine;

namespace Zelda.Game.Movements
{
    abstract class Movement
    {
        Point _xy;
        [Description("이 이동 객체에 의해 조종되는 오브젝트의 좌표")]
        public Point XY
        {
            get
            {
                if (_drawable != null)
                    return _drawable.XY;

                return _xy; 
            }
        }

        public int X
        {
            get { return XY.X; }
        }

        public int Y
        {
            get { return XY.Y; }
        }

        public bool IsStopped
        {
            get { return !IsStarted; }
        }

        public virtual bool IsStarted
        {
            get { return false; }
        }

        public virtual bool IsFinished
        {
            get { return false; }
        }

        Script.Movement _scriptMovement;
        public Script.Movement ScriptMovement
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

        Drawable _drawable;
        public Drawable Drawable
        {
            get { return _drawable; }
        }
        
        uint _lastMoveDate;     // 마지막으로 X나 Y방향으로 이동한 시각
        bool _finished;

        public void SetXY(int x, int y)
        {
            SetXY(new Point(x, y));
        }

        public void SetXY(Point xy)
        {
            if (_drawable != null)
                _drawable.XY = xy;

            _xy = xy;

            NotifyPositionChanged();
            _lastMoveDate = EngineSystem.Now;
        }

        public virtual void Stop()
        {
        }

        // X나 Y가 변경되었을 때 호출합니다
        public virtual void NotifyPositionChanged()
        {
            if (_scriptMovement != null)
                _scriptMovement.NotifyPositionChanged(XY);
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
        }

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

        public void SetDrawable(Drawable drawable)
        {
            _drawable = drawable;

            if (drawable == null)
                _xy = new Point(0, 0);
            else
                _xy = drawable.XY;
            
            NotifyObjectControlled();
        }

        public virtual void NotifyObjectControlled()
        {
        }
    }
}
