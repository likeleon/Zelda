using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Game
    {
        private readonly MainLoop _mainLoop;
        public MainLoop MainLoop
        {
            get { return _mainLoop; }
        }

        private bool _started;

        public Game(MainLoop mainLoop)
        {
            _mainLoop = mainLoop;
        }

        public bool NotifyInput(InputEvent inputEvent)
        {
            return true;
        }

        public void Start()
        {
            if (_started)
                return;

            _started = true;
        }

        public void Stop()
        {
            if (!_started)
                return;

            _started = false;
        }

        public void Update()
        {
            if (!_started)
                return;
        }

        public void Draw(Surface dstSurface)
        {
        }
    }
}
