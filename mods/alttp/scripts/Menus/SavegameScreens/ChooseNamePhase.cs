using System;
using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class ChooseNamePhase : IPhase
    {
        readonly SavegameScreen _screen;
        readonly ScriptTextSurface _playerNameText;
        readonly ScriptSurface _lettersImg;
        readonly ScriptSprite _nameArrowSprite;
        
        string _playerName = string.Empty;
        Point _letterCursor;
        bool _canAddLetterPlayerName = true;

        public string Name { get { return "choose_name"; } }

        public ChooseNamePhase(SavegameScreen screen)
        {
            _screen = screen;

            _screen.TitleText.SetTextKey("selection_menu.phase.choose_name");
            _screen.CursorSprite.SetAnimation("letters");
            var font = Fonts.GetMenuFont();
            _playerNameText = ScriptTextSurface.Create(font: font.Id, fontSize: font.Size);
            _lettersImg = ScriptSurface.Create("menus/selection_menu_letters.png");
            _nameArrowSprite = ScriptSprite.Create("menus/arrow");
            _nameArrowSprite.SetDirection(Direction4.Right);
        }

        public void OnDraw()
        {
            _screen.CursorSprite.Draw(
                _screen.Surface, 
                51 + 16 * _letterCursor.X, 
                93 + 18 * _letterCursor.Y);

            _nameArrowSprite.Draw(_screen.Surface, 57, 76);
            _playerNameText.Draw(_screen.Surface, 67, 85);
            _lettersImg.Draw(_screen.Surface, 57, 98);
        }

        public bool DirectionPressed(Direction8 direction8)
        {
            var handled = true;
            if (direction8 == Direction8.Right)
            {
                Core.Audio.PlaySound("cursor");
                _letterCursor.X = (_letterCursor.X + 1) % 13;
            }
            else if (direction8 == Direction8.Up)
            {
                Core.Audio.PlaySound("cursor");
                _letterCursor.Y = (_letterCursor.Y + 4) % 5;
            }
            else if (direction8 == Direction8.Left)
            {
                Core.Audio.PlaySound("cursor");
                _letterCursor.X = (_letterCursor.X + 12) % 13;
            }
            else if (direction8 == Direction8.Down)
            {
                Core.Audio.PlaySound("cursor");
                _letterCursor.Y = (_letterCursor.Y + 1) % 5;
            }
            else
                handled = false;
            return handled;
        }

        public bool KeyPressed(KeyboardKey key)
        {
            var handled = false;
            var finished = false;
            if (key == KeyboardKey.Return)
            {
                finished = ValidatePlayerName();
                handled = true;
            }
            else if (key == KeyboardKey.Space || key == KeyboardKey.C)
            {
                if (_canAddLetterPlayerName)
                {
                    finished = AddLetterPlayerName();
                    _playerNameText.SetText(_playerName);
                    _canAddLetterPlayerName = false;
                    ScriptTimer.Start(_screen, 300, () => _canAddLetterPlayerName = true);
                    handled = true;
                }
            }

            if (finished)
                _screen.InitPhaseSelectFile();

            return handled;
        }

        bool AddLetterPlayerName()
        {
            var size = _playerName.Length;
            var letterCursor = _letterCursor;
            char letterToAdd = '\0';
            var finished = false;

            if (letterCursor.Y == 0) // A to M
                letterToAdd = (char)('A' + letterCursor.X);
            else if (letterCursor.Y == 1) // N to Z
                letterToAdd = (char)('N' + letterCursor.X);
            else if (letterCursor.Y == 2) // a to m
                letterToAdd = (char)('a' + letterCursor.X);
            else if (letterCursor.Y == 3) // n to z
                letterToAdd = (char)('n' + letterCursor.X);
            else if (letterCursor.Y == 4)  // 숫자나 특수기호
            {
                if (letterCursor.X <= 9)
                {
                    // 숫자
                    letterToAdd = (char)('0' + letterCursor.X);
                }
                else
                {
                    // 특수기호
                    if (letterCursor.X == 10)   // 마지막 문자 삭제
                    {
                        if (size == 0)
                            Core.Audio.PlaySound("wrong");
                        else
                        {
                            Core.Audio.PlaySound("danger");
                            _playerName = _playerName.Substring(0, size - 1);
                        }
                    }
                    else if (letterCursor.X == 11)  // 유효성 검사
                    {
                        finished = ValidatePlayerName();
                    }
                    else if (letterCursor.X == 12)  // 취소
                    {
                        Core.Audio.PlaySound("danger");
                        finished = true;
                    }
                }
            }

            if (letterToAdd != '\0')
            {
                if (size < 6)
                {
                    Core.Audio.PlaySound("danger");
                    _playerName = _playerName + letterToAdd;
                }
                else
                    Core.Audio.PlaySound("wrong");
            }

            return finished;
        }

        bool ValidatePlayerName()
        {
            if (_playerName.Length == 0)
            {
                Core.Audio.PlaySound("wrong");
                return false;
            }

            Core.Audio.PlaySound("ok");

            var savegame = _screen.Slots[_screen.CursorPosition - 1].Savegame;
            _screen.SetInitialValues(savegame, _playerName);
            savegame.Save();
            _screen.ReadSavegames();
            return true;
        }
    }
}
