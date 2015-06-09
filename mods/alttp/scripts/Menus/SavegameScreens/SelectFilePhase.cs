using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class SelectFilePhase : IPhase
    {
        public string Name { get { return "select_file"; } }

        public void OnDraw(SavegameScreen screen)
        {
            for (int i = 1; i <= 3; ++i)
                screen.DrawSavegame(i);

            screen.DrawBottomButtons();
            screen.DrawSavegameCursor();

            for (int i = 1; i <= 3; ++i)
                screen.DrawSavegameNumber(i);
        }

        public bool DirectionPressed(SavegameScreen screen, Direction8 direction8)
        {
            var handled = true;
            if (direction8 == Direction8.Down)
                screen.MoveCursorDown();
            else if (direction8 == Direction8.Up)
                screen.MoveCursorUp();
            else if (direction8 == Direction8.Right || direction8 == Direction8.Left)
                screen.MoveCursorLeftOrRight();
            else
                handled = false;
            return handled;
        }

        public bool KeyPressed(SavegameScreen screen, KeyboardKey key)
        {
            if (key != KeyboardKey.Space && key != KeyboardKey.Return)
                return false;

            var handled = false;
            ScriptAudio.PlaySound("ok");
            if (screen.CursorPosition == 5)
                screen.InitPhaseOptions();
            else if (screen.CursorPosition == 4)
                screen.InitPhaseEraseFile();
            else
            {
                var slot = screen.Slots[screen.CursorPosition - 1];
                if (ScriptGame.Exists(slot.FileName))
                {
                    screen.IsFinished = true;
                    screen.Surface.FadeOut();
                    ScriptTimer.Start(screen, 700, () =>
                    {
                        screen.Stop();
                        screen.Main.StartSavegame(slot.Savegame);
                    });
                }
                else
                    screen.InitPhaseChooseName();
            }

            return handled;
        }
    }
}
