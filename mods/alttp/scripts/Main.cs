using Alttp.Menus;
using Alttp.Menus.SavegameScreens;
using Alttp.Menus.TitleScreens;
using System;
using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp
{
    class Main : Zelda.Game.Main
    {
        bool _debugEnabled;
        readonly DebugConsole _console = new DebugConsole();

        public PlayGame Game { get; set; }

        protected override void OnStarted()
        {
            LoadSettings();

            _debugEnabled = Core.Mod.ModFiles.DataFileExists("debug");

            var solarusLogo = new SolarusLogo();
            var languageMenu = new LanguageMenu();
            var titleScreen = new TitleScreen(_debugEnabled);
            var savegameScreen = new SavegameScreen(this);

            solarusLogo.Finished += (_, e) =>
            {
                if (Game == null)
                    Menu.Start(this, languageMenu);
            };

            languageMenu.Finished += (_, e) =>
            {
                if (Game == null)
                    Menu.Start(this, titleScreen);
            };

            titleScreen.Finished += (_, e) =>
            {
                if (Game == null)
                    Menu.Start(this, savegameScreen);
            };

            Menu.Start(this, solarusLogo);
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
                Menu.Start(this, _console);
            else
                handled = false;

            return handled;
        }

        void StartGameIfSaveExists(int saveIndex)
        {
            if (saveIndex < 1 || saveIndex > 3)
                throw new ArgumentOutOfRangeException("saveIndex", "Should be in range of 1~3");

            var saveFileName = "save{0}.dat".F(saveIndex);
            if (!Savegame.Exists(saveFileName))
                return;

            var savegame = Savegame.Load(saveFileName);
            Menu.StopAll(this);
            StartSavegame(savegame);
        }

        public void StartSavegame(Savegame savegame)
        {
            PlayGame.Run(savegame, this);
        }

        public override void Dispose()
        {
        }
    }
}
