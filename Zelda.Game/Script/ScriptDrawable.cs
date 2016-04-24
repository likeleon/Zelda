using System;
using System.Collections.Generic;
using Zelda.Game.Lowlevel;

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
            get { return _drawable.XY; }
            set { _drawable.XY = value; }
        }

        internal ScriptDrawable(Drawable drawable)
        {
            _drawable = drawable;
        }

        public void Draw(ScriptSurface dstSurface)
        {
            Draw(dstSurface, 0, 0);
        }

        public void Draw(ScriptSurface dstSurface, Point xy)
        {
            Draw(dstSurface, xy.X, xy.Y);
        }

        public void Draw(ScriptSurface dstSurface, int x, int y)
        {
            ScriptToCore.Call(() => _drawable.Draw(dstSurface.Surface, x, y));
        }

        public void DrawRegion(Rectangle region, ScriptSurface dstSurface, Point? dstPosition = null)
        {
            ScriptToCore.Call(() => 
            {
                if (dstPosition != null)
                    _drawable.DrawRegion(region, dstSurface.Surface, dstPosition.Value);
                else
                    _drawable.DrawRegion(region, dstSurface.Surface);
            });
        }

        public void StopMovement()
        {
            ScriptToCore.Call(_drawable.StopMovement);
        }

        [CLSCompliant(false)]
        public void FadeIn(uint? delay = null, Action callback = null)
        {
            ScriptToCore.Call(() =>
            {
                TransitionFade transition = new TransitionFade(TransitionDirection.Opening, _drawable.TransitionSurface);
                transition.ClearColor();
                transition.Delay = delay ?? 20;
                _drawable.StartTransition(transition, callback);
            });
        }

        [CLSCompliant(false)]
        public void FadeOut(uint? delay = null, Action callback = null)
        {
            ScriptToCore.Call(() =>
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
            return _drawables.Contains(drawable);
        }

        internal static void AddDrawable(Drawable drawable)
        {
            ScriptToCore.Call(() =>
            {
                if (HasDrawable(drawable))
                    throw new ArgumentException("This drawable object is already registered", "drawable");

                _drawables.Add(drawable);
            });
        }

        internal static void RemoveDrawable(Drawable drawable)
        {
            ScriptToCore.Call(() =>
            {
                if (!HasDrawable(drawable))
                    throw new ArgumentException("This drawable object was not created by Mod");

                _drawables.Remove(drawable);
                _drawablesToRemove.Add(drawable);
            });
        }

        internal static void UpdateDrawables()
        {
            ScriptToCore.Call(() =>
            {
                foreach (Drawable drawable in _drawables)
                    drawable.Update();

                _drawablesToRemove.Clear();
            });
        }

        internal static void DestroyDrawables()
        {
            _drawables.Clear();
            _drawablesToRemove.Clear();
        }
        #endregion
    }
}
