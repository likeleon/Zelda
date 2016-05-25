using Alttp.Menus;
using Zelda.Game;

namespace Alttp
{
    class PlayGame : Game
    {
        public DialogBox DialogBox { get; private set; }

        public static void Run(Savegame savegame, Main main)
        {
            main.Game = savegame;
            savegame.Start(() => new PlayGame(savegame));
        }

        PlayGame(Savegame savegame)
            : base(savegame)
        {
            FixStartingLocation();
        }

        void FixStartingLocation()
        {
            // TODO
        }

        protected override void OnStarted()
        {
            DialogBox = new DialogBox(this);
            DialogBox.Initialize();
        }

        protected override void OnFinished()
        {
            DialogBox.Quit();
        }

        protected override bool OnDialogStarted(Dialog dialog, object info)
        {
            return DialogBox.OnDialogStarted(dialog, info);
        }

        protected override void OnDialogFinished(Dialog dialog)
        {
            DialogBox.OnDialogFinished(dialog);
        }
    }
}
