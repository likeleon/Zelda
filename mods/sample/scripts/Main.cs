using Sample.Menus;
using Sample.Scripts;
using System;
using Zelda.Game;

namespace Sample
{
    public class Main : Zelda.Game.Main
    {
        protected override void OnStarted()
        {
            Console.WriteLine("This is a sample mod for Zelda.");

            Core.Mod.SetLanguage("en");

            var solarusLogo = new SolarusLogo();
            solarusLogo.Finished += (o, e) => GameManager.StartGame();
            Menu.Start(this, solarusLogo);
        }

        public override void Dispose() { }
    }
}
