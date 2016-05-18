using Zelda.Game.LowLevel;

namespace Zelda.Game.Script
{
    public class ScriptSurface : ScriptDrawable
    {
        public int Width { get { return _surface.Width; } }
        public int Height { get { return _surface.Height; } }

        readonly Surface _surface;
        internal Surface Surface { get { return _surface; } }

        public static ScriptSurface Create()
        {
            return ScriptToCore.Call(() => Create(Core.Video.ModSize.Width, Core.Video.ModSize.Height));
        }

        public static ScriptSurface Create(Size size)
        {
            return ScriptToCore.Call(() => Create(size.Width, size.Height));
        }

        public static ScriptSurface Create(int width, int height)
        {
            return ScriptToCore.Call(() =>
            {
                var surface = Surface.Create(width, height);
                return CreateScriptSurface(surface);
            });
        }

        public static ScriptSurface Create(string fileName, bool languageSpecific = false)
        {
            return ScriptToCore.Call(() =>
            {
                var surface = Surface.Create(fileName, languageSpecific ? Surface.ImageDirectory.Language : Surface.ImageDirectory.Sprites);
                return CreateScriptSurface(surface);
            });
        }

        static ScriptSurface CreateScriptSurface(Surface surface)
        {
            ScriptDrawable.AddDrawable(surface);
            return surface.ScriptSurface;
        }

        internal ScriptSurface(Surface surface)
            : base(surface)
        {
            _surface = surface;
        }

        public void SetOpacity(byte opacity)
        {
            ScriptToCore.Call(() => _surface.SetOpacity(opacity));
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
