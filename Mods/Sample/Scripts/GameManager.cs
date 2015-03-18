using Zelda.Game.Script;

namespace Sample.Scripts
{
    class GameManager
    {
        public static void StartGame()
        {
            bool exists = Game.Exists("Save1.dat");
            Game game = Game.Load("Save1.dat");
            if (!exists)
            {
                game.MaxLife = 12;
                game.Life = game.MaxLife;
            }
            game.Start();
        }
    }
}
