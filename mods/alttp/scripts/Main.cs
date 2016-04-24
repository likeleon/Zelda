using Alttp.Menus;
using Alttp.Menus.SavegameScreens;
using Alttp.Menus.TitleScreens;
using System;
using Zelda.Game;
using Zelda.Game.Lowlevel;
using Zelda.Game.Script;

namespace Alttp
{
    class Main : ScriptMain
    {
        bool _debugEnabled;
        readonly DebugConsole _console = new DebugConsole();

        public PlayGame Game { get; set; }

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
                if (Game == null)
                    ScriptMenu.Start(this, languageMenu);
            };

            languageMenu.Finished += (_, e) =>
            {
                if (Game == null)
                    ScriptMenu.Start(this, titleScreen);
            };

            titleScreen.Finished += (_, e) =>
            {
                if (Game == null)
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

            if (key == KeyboardKey.F1)
                StartGameIfSaveExists(1);
            else if (key == KeyboardKey.F2)
                StartGameIfSaveExists(2);
            else if (key == KeyboardKey.F3)
                StartGameIfSaveExists(3);
            else if (key == KeyboardKey.F12 && !_console.IsEnabled)
                ScriptMenu.Start(this, _console);
            else
                handled = false;

            return handled;
        }

        void StartGameIfSaveExists(int saveIndex)
        {
            if (saveIndex < 1 || saveIndex > 3)
                throw new ArgumentOutOfRangeException("saveIndex", "Should be in range of 1~3");

            var saveFileName = "save{0}.dat".F(saveIndex);
            if (ScriptGame.Exists(saveFileName))
            {
                Game = ScriptGame.Load<PlayGame>(saveFileName);
                ScriptMenu.StopAll(this);
                StartSavegame(Game);
            }
        }

        public void StartSavegame(PlayGame game)
        {
            game.Play(this);
        }
    }
}
