using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class SelectFilePhase : IPhase
    {
        readonly SavegameScreen _screen;

        public string Name { get { return "select_file"; } }

        public SelectFilePhase(SavegameScreen screen)
        {
            _screen = screen;
            _screen.TitleText.SetTextKey("selection_menu.phase.select_file");
            _screen.SetBottomButtons("selection_menu.erase", "selection_menu.options");
            _screen.CursorSprite.SetAnimation("blue");
        }

        public void OnDraw()
        {
            for (int i = 1; i <= 3; ++i)
                _screen.DrawSavegame(i);

            _screen.DrawBottomButtons();
            _screen.DrawSavegameCursor();

            for (int i = 1; i <= 3; ++i)
                _screen.DrawSavegameNumber(i);
        }

        public bool DirectionPressed(Direction8 direction8)
        {
            var handled = true;
            if (direction8 == Direction8.Down)
                _screen.MoveCursorDown();
            else if (direction8 == Direction8.Up)
                _screen.MoveCursorUp();
            else if (direction8 == Direction8.Right || direction8 == Direction8.Left)
                _screen.MoveCursorLeftOrRight();
            else
                handled = false;
            return handled;
        }

        public bool KeyPressed(KeyboardKey key)
        {
            if (key != KeyboardKey.Space && key != KeyboardKey.Return)
                return false;

            var handled = false;
            Core.Audio.PlaySound("ok");
            if (_screen.CursorPosition == 5)
                _screen.InitPhaseOptions();
            else if (_screen.CursorPosition == 4)
                _screen.InitPhaseEraseFile();
            else
            {
                var slot = _screen.Slots[_screen.CursorPosition - 1];
                if (ScriptGame.Exists(slot.FileName))
                {
                    _screen.IsFinished = true;
                    _screen.Surface.FadeOut();
                    ScriptTimer.Start(_screen, 700, () =>
                    {
                        _screen.Stop();
                        _screen.Main.StartSavegame(slot.Savegame);
                    });
                }
                else
                    _screen.InitPhaseChooseName();
            }

            return handled;
        }
    }
}
