using Alttp.Menus;
using Zelda.Game.Script;

namespace Alttp
{
    public class Main : ScriptMain
    {
        bool _debugEnabled;

        protected override void OnStarted()
        {
            LoadSettings();

            _debugEnabled = ScriptFile.Exists("debug");

            var solarusLogo = new SolarusLogo();
            ScriptMenu.Start(this, solarusLogo);
        }

        protected override void OnFinished()
        {
            SaveSettings();
        }
    }
}
