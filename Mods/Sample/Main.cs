using System;
using Zelda.Game;
using Zelda.Game.Script;

namespace Sample
{
    public class Main : ScriptMain
    {
        public Main(MainLoop mainLoop)
            : base(mainLoop)
        {
        }

        protected override void OnStarted()
        {
            Console.WriteLine("This is a sample mod for Zelda.");

            Language.LanguageCode = "en";
        }
    }
}
