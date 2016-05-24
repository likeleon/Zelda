using Sample.Menus;
using Sample.Scripts;
using System;
using Zelda.Game;
using Zelda.Game.Script;

namespace Sample
{
    public class Main : ScriptMain
    {
        protected override void OnStarted()
        {
            Console.WriteLine("This is a sample mod for Zelda.");

            Core.Mod.SetLanguage("en");

            var solarusLogo = new SolarusLogo();
            solarusLogo.Finished += (o, e) => GameManager.StartGame();
            Menu.Start(this, solarusLogo);
        }
    }
}
