using Zelda.Game;

namespace Sample.Scripts
{
    class GameManager
    {
        public static void StartGame()
        {
            var exists = Savegame.Exists("save1.dat");
            var savegame = new Savegame("save1.dat");
            if (!exists)
            {
                savegame.SetMaxLife(12);
                savegame.SetLife(savegame.GetMaxLife());
                savegame.SetAbility(Ability.Lift, 2);
            }
            savegame.Start(() => new Game(savegame));
        }
    }
}
