using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class ConfirmErasePhase : IPhase
    {
        readonly SavegameScreen _screen;
        readonly int _saveNumberToErase;

        public string Name { get { return "confirm_erase"; } }

        public ConfirmErasePhase(SavegameScreen screen)
        {
            _screen = screen;
            _screen.TitleText.SetTextKey("selection_menu.phase.confirm_erase");
            _screen.SetBottomButtons("selection_menu.big_no", "selection_menu.big_yes");
            _saveNumberToErase = _screen.CursorPosition;
            _screen.CursorPosition = 4; // 기본으로 "no" 선택
        }

        public void OnDraw()
        {
            _screen.DrawSavegame(_saveNumberToErase);
            _screen.DrawSavegameNumber(_saveNumberToErase);

            _screen.DrawBottomButtons();
            _screen.DrawSavegameCursor();
        }

        public bool DirectionPressed(Direction8 direction8)
        {
            if (direction8 == Direction8.Right || direction8 == Direction8.Left)
            {
                _screen.MoveCursorLeftOrRight();
                return true;
            }
            return false;
        }

        public bool KeyPressed(Zelda.Game.LowLevel.KeyboardKey key)
        {
            if (key != KeyboardKey.Space && key != KeyboardKey.Return)
                return false;

            if (_screen.CursorPosition == 5)
            {
                ScriptAudio.PlaySound("boss_killed");
                var slot = _screen.Slots[_saveNumberToErase - 1];
                ScriptGame.Delete(slot.FileName);
                _screen.CursorPosition = _saveNumberToErase;
                _screen.ReadSavegames();
                _screen.InitPhaseSelectFile();
            }
            else if (_screen.CursorPosition == 4)
            {
                ScriptAudio.PlaySound("ok");
                _screen.InitPhaseSelectFile();
            }
            return true;
        }
    }
}
