using System;
using System.Collections.Generic;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        static readonly HashSet<Drawable> _drawables = new HashSet<Drawable>();
        static readonly HashSet<Drawable> _drawablesToRemove = new HashSet<Drawable>();

        public static bool HasDrawable(Drawable drawable)
        {
            return _drawables.Contains(drawable);
        }

        public static void AddDrawable(Drawable drawable)
        {
            if (HasDrawable(drawable))
                throw new ArgumentException("This drawable object is already registered", "drawable");

            _drawables.Add(drawable);
        }

		public static void RemoveDrawable(Drawable drawable)
        {
            if (!HasDrawable(drawable))
                throw new ArgumentException("This drawable object was not created by Mod");

            _drawables.Remove(drawable);
            _drawablesToRemove.Add(drawable);
        }

		static void UpdateDrawables()
        {
            foreach (Drawable drawable in _drawables)
                drawable.Update();

            _drawablesToRemove.Clear();
        }

		static void DestroyDrawables()
        {
            _drawables.Clear();
            _drawablesToRemove.Clear();
        }
    }
}
