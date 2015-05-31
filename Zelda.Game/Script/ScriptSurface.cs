using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptSurface : ScriptDrawable
    {
        public void SetOpacity(byte opacity)
        {
            ScriptToCore.Call(() => _surface.SetOpacity(opacity));
        }

        public int Width { get { return _surface.Width; } }
        public int Height { get { return _surface.Height; } }

        readonly Surface _surface;
        internal Surface Surface
        {
            get { return _surface; }
        }

        public static ScriptSurface Create()
        {
            return ScriptToCore.Call(() => Create(Video.ModSize.Width, Video.ModSize.Height));
        }

        public static ScriptSurface Create(int width, int height)
        {
            return ScriptToCore.Call(() =>
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
            ScriptToCore.Call(_surface.Clear);
        }

        public void FillColor(Color color, Rectangle? where = null)
        {
            ScriptToCore.Call(() => _surface.FillWithColor(color, where));
        }
    }
}
