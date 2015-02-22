using System;
using Zelda.Game.Engine;
using RawSurface = Zelda.Game.Engine.Surface;

namespace Zelda.Game.Script
{
    public class Surface
    {
        readonly RawSurface _rawSurface;

        public static Surface Create()
        {
            return Create(Video.ModSize.Width, Video.ModSize.Height);
        }

        public static Surface Create(int width, int height)
        {
            RawSurface rawSurface = RawSurface.Create(width, height);
            if (rawSurface == null)
                return null;

            ScriptContext.AddDrawable(rawSurface);
            return new Surface(rawSurface);
        }

        Surface(RawSurface rawSurface)
        {
            if (rawSurface == null)
                throw new ArgumentNullException("rawSurface");

            _rawSurface = rawSurface;
        }

        public void Clear()
        {
            _rawSurface.Clear();
        }
    }
}
