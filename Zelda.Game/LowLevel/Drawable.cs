using System;
using System.Collections.Generic;
using Zelda.Game.Movements;

namespace Zelda.Game.LowLevel
{
    public abstract class Drawable
    {
        public Point XY { get; set; }
        internal Movement Movement { get; private set; }
        internal bool IsSuspended { get; private set; }
        internal Transition Transition { get; private set; }
        internal abstract Surface TransitionSurface { get; }

        static readonly HashSet<Drawable> _drawables = new HashSet<Drawable>();
        static readonly HashSet<Drawable> _drawablesToRemove = new HashSet<Drawable>();

        Action _transitionCallback;

        internal static bool HasDrawable(Drawable drawable)
        {
            return _drawables.Contains(drawable);
        }

        internal static void AddDrawable(Drawable drawable)
        {
            if (HasDrawable(drawable))
                throw new ArgumentException("This drawable object is already registered", nameof(drawable));

            _drawables.Add(drawable);
        }

        internal static void RemoveDrawable(Drawable drawable)
        {
            if (!HasDrawable(drawable))
                throw new ArgumentException("This drawable object was not created by Mod");

            _drawables.Remove(drawable);
            _drawablesToRemove.Add(drawable);
        }

        internal static void UpdateDrawables()
        {
            _drawables.Do(d => d.Update());
            _drawablesToRemove.Clear();
        }

        internal static void DestroyDrawables()
        {
            _drawables.Clear();
            _drawablesToRemove.Clear();
        }

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

        internal void StartMovement(Movement movement)
        {
            StopMovement();
            Movement = movement;
            movement.SetDrawable(this);

            movement.SetSuspended(IsSuspended);
        }

        internal void StopMovement()
        {
            Movement = null;
        }

        public void FadeIn(int? delay = null, Action callback = null)
        {
            var transition = new TransitionFade(TransitionDirection.Opening, TransitionSurface);
            StartTransitionFade(transition, delay, callback);
        }

        public void FadeOut(int? delay = null, Action callback = null)
        {
            var transition = new TransitionFade(TransitionDirection.Closing, TransitionSurface);
            StartTransitionFade(transition, delay, callback);
        }

        void StartTransitionFade(TransitionFade fade, int? delay, Action callback)
        {
            fade.ClearColor();
            fade.Delay = delay ?? 20;
            StartTransition(fade, callback);
        }
        
        internal void StartTransition(Transition transition, Action callback)
        {
            StopTransition();
            Transition = transition;
            _transitionCallback = callback;
            Transition.Start();
            Transition.SetSuspended(IsSuspended);
        }

        internal void StopTransition()
        {
            Transition = null;
            _transitionCallback = null;
        }

        internal virtual void Update()
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

            if (Movement != null)
            {
                Movement.Update();
                if (Movement != null && Movement.IsFinished)
                    StopMovement();
            }
        }

        internal virtual void SetSuspended(bool suspended)
        {
            if (IsSuspended == suspended)
                return;

            IsSuspended = suspended;

            if (Transition != null)
                Transition.SetSuspended(suspended);

            if (Movement != null)
                Movement.SetSuspended(suspended);
        }

        internal abstract void RawDraw(Surface dstSurface, Point dstPosition);
        internal abstract void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition);
        internal abstract void DrawTransition(Transition transition);
    }
}
