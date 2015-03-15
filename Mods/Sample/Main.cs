using Sample.Menus;
using System;
using Zelda.Game;
using Zelda.Game.Script;

namespace Sample
{
    public class Main : Zelda.Game.Script.Main
    {
        protected override void OnStarted()
        {
            Console.WriteLine("This is a sample mod for Zelda.");

            Language.LanguageCode = "en";

            ZeldaLogo zeldaLogo = new ZeldaLogo();
            zeldaLogo.Finished += (o, e) => Reset();
            Menu.Start(this, zeldaLogo);
        }
    }
}
