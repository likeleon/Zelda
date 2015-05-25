using Zelda.Game;
using Zelda.Game.Script;

namespace Sample.Scripts
{
    class GameManager
    {
        public static void StartGame()
        {
            bool exists = ScriptGame.Exists("save1.dat");
            ScriptGame game = ScriptGame.Load("save1.dat");
            if (!exists)
            {
                game.MaxLife = 12;
                game.Life = game.MaxLife;
                game.SetAbility(Ability.Lift, 2);
            }
            game.Start();
        }
    }
}
