using System;
using System.Linq;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus
{
    class Savegames : ScriptMenu
    {
        class Slot
        {
            readonly ScriptTextSurface _playerNameText;

            public string FileName { get; private set; }
            public ScriptGame Savegame { get; private set; }
            public ScriptSurface NumberImg { get; private set; }
            public ScriptTextSurface PlayerNameText { get { return _playerNameText; } }

            public Slot(int index)
            {
                FileName = "save{0}.dat".F(index);
                Savegame = ScriptGame.Load(FileName);
                NumberImg = ScriptSurface.Create("menus/selection_menu_save{0}.png".F(index));

                var dialogFont = LanguageFonts.GetDialogFont();
                _playerNameText = ScriptTextSurface.Create(font: dialogFont.Item1, fontSize: dialogFont.Item2);

                if (ScriptGame.Exists(FileName))
                {
                    _playerNameText.SetText(Savegame.GetStringValue("player_name"));
                    //TODO 하트 보여주기
                }
                else
                {
                    var name = "- {0} -".F(ScriptLanguage.GetString("selection_menu.empty"));
                    _playerNameText.SetText(name);
                }
            }
        }

        interface IPhase
        {
            string Name { get; }
            void OnDraw(Savegames savegames);
            bool DirectionPressed(Savegames savegames, Direction8 direction8);
            bool KeyPressed(Savegames savegames, KeyboardKey key);
        }

        class SelectFilePhase : IPhase
        {
            public string Name { get { return "select_file"; } }

            public void OnDraw(Savegames savegames)
            {
                for (int i = 1; i <= 3; ++i)
                    savegames.DrawSavegame(i);
                
                savegames.DrawBottomButtons();
                savegames.DrawSavegameCursor();

                for (int i = 1; i <= 3; ++i)
                    savegames.DrawSavegameNumber(i);
            }

            public bool DirectionPressed(Savegames savegames, Direction8 direction8)
            {
                var handled = true;
                if (direction8 == Direction8.Down)
                    savegames.MoveCursorDown();
                else if (direction8 == Direction8.Up)
                    savegames.MoveCursorUp();
                else if (direction8 == Direction8.Right || direction8 == Direction8.Left)
                    savegames.MoveCursorLeftOrRight();
                else
                    handled = false;
                return handled;
            }

            public bool KeyPressed(Savegames savegames, KeyboardKey key)
            {
                if (key != KeyboardKey.Space && key != KeyboardKey.Return)
                    return false;

                var handled = false;
                ScriptAudio.PlaySound("ok");
                if (savegames._cursorPosition == 5)
                    savegames.InitPhaseOptions();
                else if (savegames._cursorPosition == 4)
                    savegames.InitPhaseEraseFile();
                else
                {
                    var slot = savegames._slots[savegames._cursorPosition - 1];
                    if (ScriptGame.Exists(slot.FileName))
                    {
                        savegames._finished = true;
                        savegames._surface.FadeOut();
                        ScriptTimer.Start(savegames, 700, () =>
                        {
                            savegames.Stop();
                            savegames._main.StartSavegame(slot.Savegame);
                        });
                    }
                    else
                        savegames.InitPhaseChooseName();
                }

                return handled;
            }
        }

        static readonly int _cloudWidth = 111;
        static readonly int _cloudHeight = 88;

        readonly Main _main;

        ScriptSurface _surface;
        Color _backgroundColor;
        
        ScriptSurface _backgroundImg;
        ScriptSurface _cloudImg;
        ScriptSurface _saveContainerImg;
        ScriptSurface _optionContainerImg;
        
        ScriptTextSurface _option1Text;
        ScriptTextSurface _option2Text;
        ScriptTextSurface _titleText;
        
        int _cursorPosition = 1;
        ScriptSprite _cursorSprite;
        bool _allowCursorMove = true;
        bool _finished;
        Point[] _cloudPositions;

        Slot[] _slots;

        IPhase _phase;

        public Savegames(Main main)
        {
            _main = main;
        }

        protected override void OnStarted()
        {
            _surface = ScriptSurface.Create(320, 240);
            _backgroundColor = new Color(104, 144, 240);
            _backgroundImg = ScriptSurface.Create("menus/selection_menu_background.png");
            _cloudImg = ScriptSurface.Create("menus/selection_menu_cloud.png");
            _saveContainerImg = ScriptSurface.Create("menus/selection_menu_save_container.png");
            _optionContainerImg = ScriptSurface.Create("menus/selection_menu_option_container.png");

            var dialogFont = LanguageFonts.GetDialogFont();
            var menuFont = LanguageFonts.GetMenuFont();
            _option1Text = ScriptTextSurface.Create(font: dialogFont.Item1, fontSize: dialogFont.Item2);
            _option2Text = ScriptTextSurface.Create(font: dialogFont.Item1, fontSize: dialogFont.Item2);
            _titleText = ScriptTextSurface.Create(font: menuFont.Item1, fontSize: menuFont.Item2, horizontalAlignment: TextHorizontalAlignment.Center);

            _cursorSprite = ScriptSprite.Create("menus/selection_menu_cursor");

            _cloudPositions = new Point[]
            {
                new Point( 20,  40),
                new Point( 50, 160),
                new Point(160,  30),
                new Point(270, 200),
                new Point(200, 120),
                new Point( 90, 120),
                new Point(300, 100),
                new Point(240,  10),
                new Point( 60, 190),
                new Point(150, 120),
                new Point(310, 220),
                new Point( 70,  20),
                new Point(130, 180),
                new Point(200, 700),
                new Point( 20, 120),
                new Point(170, 220)
            };

            RepeatMoveClouds();

            ReadSavegames();
            ScriptAudio.PlayMusic("game_over");
            InitPhaseSelectFile();

            _surface.FadeIn();
        }

        void RepeatMoveClouds()
        {
            for (int i = 0; i < _cloudPositions.Length; ++i)
            {
                var position = _cloudPositions[i];
                position.X += 1;
                if (position.X >= _surface.Width)
                    position.X = 0;

                position.Y -= 1;
                if (position.Y <= -_cloudHeight)
                    position.Y = _surface.Height - _cloudHeight;

                _cloudPositions[i] = position;
            }

            ScriptTimer.Start(this, 100, (Action)RepeatMoveClouds);
        }

        void ReadSavegames()
        {
            _slots = Enumerable.Range(1, 3).Select(i => new Slot(i)).ToArray();
        }

        void InitPhaseSelectFile()
        {
            _phase = new SelectFilePhase();
            _titleText.SetTextKey("selection_menu.phase.select_file");
            SetBottomButtons("selection_menu.erase", "selection_menu.options");
            _cursorSprite.SetAnimation("blue");
        }

        void InitPhaseOptions()
        {
        }

        void InitPhaseEraseFile()
        {
        }

        void InitPhaseChooseName()
        {
        }

        void SetBottomButtons(string key1, string key2)
        {
            _option1Text.SetTextKey(key1 ?? string.Empty);
            _option2Text.SetTextKey(key2 ?? string.Empty);
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            _surface.FillColor(_backgroundColor);

            var width = _surface.Width;
            var height = _surface.Height;
            foreach (var position in _cloudPositions)
            {
                _cloudImg.Draw(_surface, position);

                if (position.X >= width - _cloudWidth)
                {
                    _cloudImg.Draw(_surface, new Point(position.X - width, position.Y));
                    if (position.Y <= 0)
                        _cloudImg.Draw(_surface, position.X - width, position.Y + height);
                }

                if (position.Y <= 0)
                    _cloudImg.Draw(_surface, new Point(position.X, position.Y + height));
            }

            _backgroundImg.Draw(_surface, 37, 38);
            _titleText.Draw(_surface, 160, 54);

            _phase.OnDraw(this);

            _surface.Draw(dstSurface, dstSurface.Width / 2 - 160, dstSurface.Height / 2 - 120);
        }

        void DrawSavegame(int slotIndex)
        {
            _saveContainerImg.Draw(_surface, 57, 48 + slotIndex * 27);
            _slots[slotIndex - 1].PlayerNameText.Draw(_surface, 87, 61 + slotIndex * 27);
            // TODO: 하트 그리기
        }

        void DrawBottomButtons()
        {
            if (!String.IsNullOrEmpty(_option1Text.Text))
            {
                _optionContainerImg.Draw(_surface, 57, 158);
                _option1Text.Draw(_surface, 90, 172);
            }
            if (!String.IsNullOrEmpty(_option2Text.Text))
            {
                _optionContainerImg.Draw(_surface, 165, 158);
                _option2Text.Draw(_surface, 198, 172);
            }
        }

        void DrawSavegameCursor()
        {
            var x = (_cursorPosition == 5) ? 166 : 58;
            var y = (_cursorPosition < 4) ? 49 + _cursorPosition * 27 : 159;
            _cursorSprite.Draw(_surface, x, y);
        }

        void DrawSavegameNumber(int slotIndex)
        {
            var slot = _slots[slotIndex - 1];
            slot.NumberImg.Draw(_surface, 62, 53 + slotIndex * 27);
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            var handled = false;
            if (key == KeyboardKey.Escape)
            {
                handled = true;
                Main.Exit();
            }
            else if (key == KeyboardKey.Right)
                handled = DirectionPressed(Direction8.Right);
            else if (key == KeyboardKey.Up)
                handled = DirectionPressed(Direction8.Up);
            else if (key == KeyboardKey.Left)
                handled = DirectionPressed(Direction8.Left);
            else if (key == KeyboardKey.Down)
                handled = DirectionPressed(Direction8.Down);
            else if (!_finished)
                _phase.KeyPressed(this, key);

            return handled;
        }

        bool DirectionPressed(Direction8 direction8)
        {
            if (!_allowCursorMove || _finished)
                return false;

            _allowCursorMove = false;
            ScriptTimer.Start(this, 100, () => _allowCursorMove = true);

            return _phase.DirectionPressed(this, direction8);
        }

        void MoveCursorUp()
        {
            ScriptAudio.PlaySound("cursor");
            var cursorPosition = _cursorPosition - 1;
            if (cursorPosition == 0)
                cursorPosition = 4;
            else if (cursorPosition == 4)
                cursorPosition = 3;
            SetCursorPosition(cursorPosition);
        }

        void SetCursorPosition(int cursorPosition)
        {
            _cursorPosition = cursorPosition;
            _cursorSprite.SetFrame(0);
        }

        void MoveCursorDown()
        {
            ScriptAudio.PlaySound("cursor");
            var cursorPosition = _cursorPosition + 1;
            if (cursorPosition >= 5)
                cursorPosition = 1;
            SetCursorPosition(cursorPosition);
        }

        void MoveCursorLeftOrRight()
        {
            if (_cursorPosition == 4)
            {
                ScriptAudio.PlaySound("cursor");
                SetCursorPosition(5);
            }
            else if (_cursorPosition == 5)
            {
                ScriptAudio.PlaySound("cursor");
                SetCursorPosition(4);
            }
        }
    }
}
