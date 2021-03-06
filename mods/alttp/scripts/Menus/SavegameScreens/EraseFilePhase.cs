﻿using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.SavegameScreens
{
    class EraseFilePhase : IPhase
    {
        readonly SavegameScreen _screen;

        public string Name { get { return "erase_file"; } }

        public EraseFilePhase(SavegameScreen screen)
        {
            _screen = screen;
            _screen.TitleText.SetTextKey("selection_menu.phase.erase_file");
            _screen.SetBottomButtons("selection_menu.cancel", null);
            _screen.CursorSprite.SetAnimation("red");
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
            bool handled = true;
            if (direction8 == Direction8.Down)
                _screen.MoveCursorDown();
            else if (direction8 == Direction8.Up)
                _screen.MoveCursorUp();
            else
                return false;
            return handled;
        }

        public bool KeyPressed(KeyboardKey key)
        {
            if (key != KeyboardKey.Space && key != KeyboardKey.Return)
                return false;

            if (_screen.CursorPosition == 4)
            {
                Core.Audio?.PlaySound("ok");
                _screen.InitPhaseSelectFile();
            }
            else if (_screen.CursorPosition > 0 && _screen.CursorPosition <= 3)
            {
                var slot = _screen.Slots[_screen.CursorPosition - 1];
                if (!Savegame.Exists(slot.FileName))
                    Core.Audio?.PlaySound("wrong");
                else
                {
                    Core.Audio?.PlaySound("ok");
                    _screen.InitPhaseConfirmErase();
                }
            }
            return true;
        }
    }
}
