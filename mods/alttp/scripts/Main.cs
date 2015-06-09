using Alttp.Menus;
using Alttp.Menus.SavegameScreens;
using Alttp.Menus.TitleScreens;
using System;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp
{
    public class Main : ScriptMain
    {
        ScriptGame _game;
        bool _debugEnabled;

        protected override void OnStarted()
        {
            LoadSettings();

            _debugEnabled = ScriptFile.Exists("debug");

            var solarusLogo = new SolarusLogo();
            var languageMenu = new LanguageMenu();
            var titleScreen = new TitleScreen(_debugEnabled);
            var savegameScreen = new SavegameScreen(this);

            solarusLogo.Finished += (_, e) =>
            {
                if (_game == null)
                    ScriptMenu.Start(this, languageMenu);
            };

            languageMenu.Finished += (_, e) =>
            {
                if (_game == null)
                    ScriptMenu.Start(this, titleScreen);
            };

            titleScreen.Finished += (_, e) =>
            {
                if (_game == null)
                    ScriptMenu.Start(this, savegameScreen);
            };

            ScriptMenu.Start(this, solarusLogo);
        }

        protected override void OnFinished()
        {
            SaveSettings();
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            bool handled = false;

            if (_debugEnabled)
                handled = DebugOnKeyPressed(key, modifiers);

            return handled;
        }

        bool DebugOnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            bool handled = true;

            return handled;
        }

        public void StartSavegame(ScriptGame game)
        {
            Console.WriteLine("Start save game here");
        }
    }
}
