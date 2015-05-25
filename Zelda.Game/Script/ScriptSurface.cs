using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptSurface : ScriptDrawable
    {
        public byte Opacity
        {
            set 
            {
                ScriptTools.ExceptionBoundaryHandle(() => 
                {
                    _surface.Opacity = value;
                });
            }
        }

        public int Width
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<int>(() =>
                {
                    return _surface.Width;
                });
            }
        }

        public int Height
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<int>(() =>
                {
                    return _surface.Height;
                });
            }
        }

        readonly Surface _surface;
        internal Surface Surface
        {
            get { return _surface; }
        }

        public static ScriptSurface Create()
        {
            return ScriptTools.ExceptionBoundaryHandle<ScriptSurface>(() =>
            {
                return Create(Video.ModSize.Width, Video.ModSize.Height);
            });
        }

        public static ScriptSurface Create(int width, int height)
        {
            return ScriptTools.ExceptionBoundaryHandle<ScriptSurface>(() =>
            {
                Surface surface = Surface.Create(width, height);
                if (surface == null)
                    return null;

                ScriptDrawable.AddDrawable(surface);
                return new ScriptSurface(surface);
            });
        }

        internal ScriptSurface(Surface surface)
            : base(surface)
        {
            _surface = surface;
        }

        public void Clear()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _surface.Clear();
            });
        }

        public void FillColor(Color color, Rectangle? where = null)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _surface.FillWithColor(color, where);
            });
        }
    }
}
