using Sample.Menus;
using Sample.Scripts;
using System;
using Zelda.Game.Script;

namespace Sample
{
    public class Main : Zelda.Game.Script.Main
    {
        protected override void OnStarted()
        {
            Console.WriteLine("This is a sample mod for Zelda.");

            ScriptLanguage.SetLanguage("en");

            ZeldaLogo zeldaLogo = new ZeldaLogo();
            zeldaLogo.Finished += (o, e) => GameManager.StartGame();
            ScriptMenu.Start(this, zeldaLogo);
        }
    }
}
