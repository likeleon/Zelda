using Alttp.Menus;
using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp
{
    class PlayGame : ScriptGame
    {
        public DialogBox DialogBox { get; private set; }

        public void Play(Main main)
        {
            main.Game = this;
            Start();
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
