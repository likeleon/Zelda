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
                game.SetMaxLife(12);
                game.SetLife(game.GetMaxLife());
                game.SetAbility(Ability.Lift, 2);
            }
            game.Start();
        }
    }
}
