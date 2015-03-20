using System;
using System.Collections.Generic;
using RawDrawable = Zelda.Game.Engine.Drawable;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        static readonly HashSet<RawDrawable> _drawables = new HashSet<RawDrawable>();
        static readonly HashSet<RawDrawable> _drawablesToRemove = new HashSet<RawDrawable>();

        public static bool HasDrawable(RawDrawable drawable)
        {
            return ScriptTools.ExceptionBoundaryHandle<bool>(() =>
            {
                return _drawables.Contains(drawable);
            });
        }

        public static void AddDrawable(RawDrawable drawable)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (HasDrawable(drawable))
                    throw new ArgumentException("This drawable object is already registered", "drawable");

                _drawables.Add(drawable);
            });
        }

		public static void RemoveDrawable(RawDrawable drawable)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (!HasDrawable(drawable))
                    throw new ArgumentException("This drawable object was not created by Mod");

                _drawables.Remove(drawable);
                _drawablesToRemove.Add(drawable);
            });
        }

		static void UpdateDrawables()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                foreach (RawDrawable drawable in _drawables)
                    drawable.Update();

                _drawablesToRemove.Clear();
            });
        }

		static void DestroyDrawables()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _drawables.Clear();
                _drawablesToRemove.Clear();
            });
        }
    }
}
