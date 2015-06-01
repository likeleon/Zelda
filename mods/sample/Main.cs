using Sample.Menus;
using Sample.Scripts;
using System;
using Zelda.Game.Script;

namespace Sample
{
    public class Main : ScriptMain
    {
        protected override void OnStarted()
        {
            Console.WriteLine("This is a sample mod for Zelda.");

            ScriptLanguage.SetLanguage("en");

            SolarusLogo solarusLogo = new SolarusLogo();
            solarusLogo.Finished += (o, e) => GameManager.StartGame();
            ScriptMenu.Start(this, solarusLogo);
        }
    }
}
