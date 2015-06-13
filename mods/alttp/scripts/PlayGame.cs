using Zelda.Game.Script;

namespace Alttp
{
    class PlayGame : ScriptGame
    {
        public void Play(Main main)
        {
            main.Game = this;
            Start();
        }
    }
}
