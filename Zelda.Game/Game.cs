using Zelda.Game.Engine;
using System;

namespace Zelda.Game
{
    class Game
    {
        readonly MainLoop _mainLoop;
        public MainLoop MainLoop
        {
            get { return _mainLoop; }
        }

        readonly SaveGame _saveGame;
        bool _started;

        public Game(MainLoop mainLoop, SaveGame saveGame)
        {
            _mainLoop = mainLoop;
            _saveGame = saveGame;

            _saveGame.Game = this;
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

            _saveGame.Game = null;

            _started = false;
        }

        public void Restart()
        {
            throw new NotImplementedException();
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
