using System;
using System.Collections.Generic;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public abstract class ScriptDrawable
    {
        readonly Drawable _drawable;
        internal Drawable Drawable
        {
            get { return _drawable; }
        }

        public Point XY
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle(() => _drawable.XY);
            }
            set 
            {
                ScriptTools.ExceptionBoundaryHandle(() => _drawable.XY = value);
            }
        }

        internal ScriptDrawable(Drawable drawable)
        {
            _drawable = drawable;
        }

        public void Draw(ScriptSurface dstSurface)
        {
            ScriptTools.ExceptionBoundaryHandle(() => Draw(dstSurface, 0, 0));
        }

        public void Draw(ScriptSurface dstSurface, int x, int y)
        {
            ScriptTools.ExceptionBoundaryHandle(() => _drawable.Draw(dstSurface.Surface, x, y));
        }

        public void StopMovement()
        {
            ScriptTools.ExceptionBoundaryHandle(_drawable.StopMovement);
        }

        [CLSCompliant(false)]
        public void FadeOut(uint? delay = null, Action callback = null)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                TransitionFade transition = new TransitionFade(TransitionDirection.Closing, _drawable.TransitionSurface);
                transition.ClearColor();
                transition.Delay = delay ?? 20;
                _drawable.StartTransition(transition, callback);
            });
        }

        #region ScriptDrawable 관리
        static readonly HashSet<Drawable> _drawables = new HashSet<Drawable>();
        static readonly HashSet<Drawable> _drawablesToRemove = new HashSet<Drawable>();

        internal static bool HasDrawable(Drawable drawable)
        {
            return ScriptTools.ExceptionBoundaryHandle(() => _drawables.Contains(drawable));
        }

        internal static void AddDrawable(Drawable drawable)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (HasDrawable(drawable))
                    throw new ArgumentException("This drawable object is already registered", "drawable");

                _drawables.Add(drawable);
            });
        }

        internal static void RemoveDrawable(Drawable drawable)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (!HasDrawable(drawable))
                    throw new ArgumentException("This drawable object was not created by Mod");

                _drawables.Remove(drawable);
                _drawablesToRemove.Add(drawable);
            });
        }

        internal static void UpdateDrawables()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                foreach (Drawable drawable in _drawables)
                    drawable.Update();

                _drawablesToRemove.Clear();
            });
        }

        internal static void DestroyDrawables()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _drawables.Clear();
                _drawablesToRemove.Clear();
            });
        }
        #endregion
    }
}
