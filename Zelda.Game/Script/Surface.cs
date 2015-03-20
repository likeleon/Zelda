using System;
using Zelda.Game.Engine;
using RawSurface = Zelda.Game.Engine.Surface;

namespace Zelda.Game.Script
{
    public class Surface : Drawable
    {
        public byte Opacity
        {
            set 
            {
                ScriptTools.ExceptionBoundaryHandle(() => 
                {
                    _rawSurface.Opacity = value;
                });
            }
        }

        public int Width
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<int>(() =>
                {
                    return _rawSurface.Width;
                });
            }
        }

        public int Height
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<int>(() =>
                {
                    return _rawSurface.Height;
                });
            }
        }

        readonly RawSurface _rawSurface;
        internal RawSurface RawSurface
        {
            get { return _rawSurface; }
        }

        public static Surface Create()
        {
            return ScriptTools.ExceptionBoundaryHandle<Surface>(() =>
            {
                return Create(Video.ModSize.Width, Video.ModSize.Height);
            });
        }

        public static Surface Create(int width, int height)
        {
            return ScriptTools.ExceptionBoundaryHandle<Surface>(() =>
            {
                RawSurface rawSurface = RawSurface.Create(width, height);
                if (rawSurface == null)
                    return null;

                ScriptContext.AddDrawable(rawSurface);
                return new Surface(rawSurface);
            });
        }

        internal Surface(RawSurface rawSurface)
            : base(rawSurface)
        {
            _rawSurface = rawSurface;
        }

        public void Clear()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _rawSurface.Clear();
            });
        }

        public void FillColor(Color color, Rectangle? where = null)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _rawSurface.FillWithColor(color, where);
            });
        }
    }
}
