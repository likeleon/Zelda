using Zelda.Game;
using Zelda.Game.Script;

namespace Sample.Scripts
{
    class GameManager
    {
        public static void StartGame()
        {
            var exists = ScriptGame.Exists("save1.dat");
            var game = ScriptGame.Load<ScriptGame>("save1.dat");
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
