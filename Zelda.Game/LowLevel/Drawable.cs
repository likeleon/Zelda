using System;
using System.Drawing;
using Zelda.Game.Movements;

namespace Zelda.Game.Lowlevel
{
    abstract class Drawable
    {
        Movement _movement;
        Action _transitionCallback;
        
        public Point XY { get; set; }
        public Movement Movement { get { return _movement; } }
        public bool IsSuspended { get; private set; }
        public Transition Transition { get; private set; }

        public abstract Surface TransitionSurface { get; }

        protected Drawable()
        {
        }

        public void Draw(Surface dstSurface)
        {
            Draw(dstSurface, new Point(0, 0));
        }

        public void Draw(Surface dstSurface, int x, int y)
        {
            Draw(dstSurface, new Point(x, y));
        }

        public void Draw(Surface dstSurface, Point dstPosition)
        {
            if (Transition != null)
                DrawTransition(Transition);

            RawDraw(dstSurface, dstPosition + XY);
        }

        public void DrawRegion(Rectangle region, Surface dstSurface)
        {
            DrawRegion(region, dstSurface, new Point(0, 0));
        }

        public void DrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            if (Transition != null)
                DrawTransition(Transition);

            RawDrawRegion(region, dstSurface, dstPosition + XY);
        }

        public void StartMovement(Movement movement)
        {
            StopMovement();
            _movement = movement;
            movement.SetDrawable(this);

            movement.SetSuspended(IsSuspended);
        }

        public void StopMovement()
        {
            _movement = null;
        }

        public void StartTransition(Transition transition, Action callback)
        {
            StopTransition();
            Transition = transition;
            _transitionCallback = callback;
            Transition.Start();
            Transition.SetSuspended(IsSuspended);
        }

        public void StopTransition()
        {
            Transition = null;
            _transitionCallback = null;
        }

        public virtual void Update()
        {
            if (Transition != null)
            {
                Transition.Update();
                if (Transition.IsFinished)
                {
                    Transition = null;

                    if (_transitionCallback != null)
                        _transitionCallback.Invoke();
                }
            }

            if (_movement != null)
            {
                _movement.Update();
                if (_movement != null && _movement.IsFinished)
                    StopMovement();
            }
        }

        public virtual void SetSuspended(bool suspended)
        {
            if (IsSuspended == suspended)
                return;

            IsSuspended = suspended;

            if (Transition != null)
                Transition.SetSuspended(suspended);

            if (_movement != null)
                _movement.SetSuspended(suspended);
        }

        public abstract void RawDraw(Surface dstSurface, Point dstPosition);
        public abstract void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition);
        public abstract void DrawTransition(Transition transition);
    }
}
