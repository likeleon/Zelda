using System;
using System.Linq;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class SavegameScreen : ScriptMenu
    {
        static readonly int _cloudWidth = 111;
        static readonly int _cloudHeight = 88;

        Color _backgroundColor;
        ScriptSurface _backgroundImg;
        ScriptSurface _cloudImg;
        ScriptSurface _saveContainerImg;
        ScriptSurface _optionContainerImg;
        
        ScriptTextSurface _option1Text;
        ScriptTextSurface _option2Text;
        
        bool _allowCursorMove = true;
        Point[] _cloudPositions;
        IPhase _phase;

        public Main Main { get; private set; }
        public int CursorPosition { get; set; }
        public Slot[] Slots { get; private set; }
        public bool IsFinished { get; set; }
        public ScriptSurface Surface { get; private set; }
        public ScriptTextSurface TitleText { get; private set; }
        public ScriptSprite CursorSprite { get; private set; }

        public SavegameScreen(Main main)
        {
            Main = main;
            CursorPosition = 1;
        }

        protected override void OnStarted()
        {
            Surface = ScriptSurface.Create(320, 240);
            _backgroundColor = new Color(104, 144, 240);
            _backgroundImg = ScriptSurface.Create("menus/selection_menu_background.png");
            _cloudImg = ScriptSurface.Create("menus/selection_menu_cloud.png");
            _saveContainerImg = ScriptSurface.Create("menus/selection_menu_save_container.png");
            _optionContainerImg = ScriptSurface.Create("menus/selection_menu_option_container.png");

            var dialogFont = LanguageFonts.GetDialogFont();
            var menuFont = LanguageFonts.GetMenuFont();
            _option1Text = ScriptTextSurface.Create(font: dialogFont.Item1, fontSize: dialogFont.Item2);
            _option2Text = ScriptTextSurface.Create(font: dialogFont.Item1, fontSize: dialogFont.Item2);
            TitleText = ScriptTextSurface.Create(font: menuFont.Item1, fontSize: menuFont.Item2, horizontalAlignment: TextHorizontalAlignment.Center);

            CursorSprite = ScriptSprite.Create("menus/selection_menu_cursor");

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

            Surface.FadeIn();
        }

        void RepeatMoveClouds()
        {
            for (int i = 0; i < _cloudPositions.Length; ++i)
            {
                var position = _cloudPositions[i];
                position.X += 1;
                if (position.X >= Surface.Width)
                    position.X = 0;

                position.Y -= 1;
                if (position.Y <= -_cloudHeight)
                    position.Y = Surface.Height - _cloudHeight;

                _cloudPositions[i] = position;
            }

            ScriptTimer.Start(this, 100, (Action)RepeatMoveClouds);
        }

        void ReadSavegames()
        {
            Slots = Enumerable.Range(1, 3).Select(i => new Slot(i)).ToArray();
        }

        public void InitPhaseSelectFile()
        {
            _phase = new SelectFilePhase(this);
        }

        public void InitPhaseOptions()
        {
        }

        public void InitPhaseEraseFile()
        {
        }

        public void InitPhaseChooseName()
        {
        }

        public void SetBottomButtons(string key1, string key2)
        {
            _option1Text.SetTextKey(key1 ?? string.Empty);
            _option2Text.SetTextKey(key2 ?? string.Empty);
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            Surface.FillColor(_backgroundColor);

            var width = Surface.Width;
            var height = Surface.Height;
            foreach (var position in _cloudPositions)
            {
                _cloudImg.Draw(Surface, position);

                if (position.X >= width - _cloudWidth)
                {
                    _cloudImg.Draw(Surface, new Point(position.X - width, position.Y));
                    if (position.Y <= 0)
                        _cloudImg.Draw(Surface, position.X - width, position.Y + height);
                }

                if (position.Y <= 0)
                    _cloudImg.Draw(Surface, new Point(position.X, position.Y + height));
            }

            _backgroundImg.Draw(Surface, 37, 38);
            TitleText.Draw(Surface, 160, 54);

            _phase.OnDraw();

            Surface.Draw(dstSurface, dstSurface.Width / 2 - 160, dstSurface.Height / 2 - 120);
        }

        public void DrawSavegame(int slotIndex)
        {
            _saveContainerImg.Draw(Surface, 57, 48 + slotIndex * 27);
            Slots[slotIndex - 1].PlayerNameText.Draw(Surface, 87, 61 + slotIndex * 27);
            // TODO: 하트 그리기
        }

        public void DrawBottomButtons()
        {
            if (!String.IsNullOrEmpty(_option1Text.Text))
            {
                _optionContainerImg.Draw(Surface, 57, 158);
                _option1Text.Draw(Surface, 90, 172);
            }
            if (!String.IsNullOrEmpty(_option2Text.Text))
            {
                _optionContainerImg.Draw(Surface, 165, 158);
                _option2Text.Draw(Surface, 198, 172);
            }
        }

        public void DrawSavegameCursor()
        {
            var x = (CursorPosition == 5) ? 166 : 58;
            var y = (CursorPosition < 4) ? 49 + CursorPosition * 27 : 159;
            CursorSprite.Draw(Surface, x, y);
        }

        public void DrawSavegameNumber(int slotIndex)
        {
            var slot = Slots[slotIndex - 1];
            slot.NumberImg.Draw(Surface, 62, 53 + slotIndex * 27);
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
            else if (!IsFinished)
                _phase.KeyPressed(key);

            return handled;
        }

        bool DirectionPressed(Direction8 direction8)
        {
            if (!_allowCursorMove || IsFinished)
                return false;

            _allowCursorMove = false;
            ScriptTimer.Start(this, 100, () => _allowCursorMove = true);

            return _phase.DirectionPressed(direction8);
        }

        public void MoveCursorUp()
        {
            ScriptAudio.PlaySound("cursor");
            var cursorPosition = CursorPosition - 1;
            if (cursorPosition == 0)
                cursorPosition = 4;
            else if (cursorPosition == 4)
                cursorPosition = 3;
            SetCursorPosition(cursorPosition);
        }

        void SetCursorPosition(int cursorPosition)
        {
            CursorPosition = cursorPosition;
            CursorSprite.SetFrame(0);
        }

        public void MoveCursorDown()
        {
            ScriptAudio.PlaySound("cursor");
            var cursorPosition = CursorPosition + 1;
            if (cursorPosition >= 5)
                cursorPosition = 1;
            SetCursorPosition(cursorPosition);
        }

        public void MoveCursorLeftOrRight()
        {
            if (CursorPosition == 4)
            {
                ScriptAudio.PlaySound("cursor");
                SetCursorPosition(5);
            }
            else if (CursorPosition == 5)
            {
                ScriptAudio.PlaySound("cursor");
                SetCursorPosition(4);
            }
        }
    }
}
