using Zelda.Game;

namespace Sample.Scripts
{
    class GameManager
    {
        public static void StartGame()
        {
            var exists = Savegame.Exists("save1.dat");
            var game = new Savegame("save1.dat");
            if (!exists)
            {
                game.SetMaxLife(12);
                game.SetLife(game.GetMaxLife());
                game.SetAbility(Ability.Lift, 2);
            }
            game.Start(new Game(game));
        }
    }
}
