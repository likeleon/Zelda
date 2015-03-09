using Zelda.Game.Engine;

namespace Zelda.Game.Movements
{
    abstract class Movement
    {
        Point _xy;
        public Point XY
        {
            get { return _xy; }
        }

        bool _isScriptCallbackEnable;
        public bool IsScriptCallbackEnable
        {
            get { return _isScriptCallbackEnable; }
            set { _isScriptCallbackEnable = value; }
        }
    }
}
