using System;
using System.Collections.Generic;
using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Alttp.Menus
{
    enum DialogBoxStyle
    {
        Box,
        Empty
    }

    enum DialogBoxSkipMode
    {
        None,
        Current,
        All,
        Unchanged
    }

    enum DialogBoxCharDelays
    {
        Slow = 60,
        Medium = 40,
        Fast = 20
    }

    enum DialogBoxVerticalPosition
    {
        Auto,
        Top,
        Bottom
    }

    enum DialogBoxSelectedAnswer
    {
        NoQuestion,
        One,
        Two
    }

    class DialogBox : ScriptMenu
    {
        static readonly int _numVisibleLines = 3;
        static readonly uint _letterSoundDelay = 100;
        static readonly Size _boxSize = new Size(220, 60);

        static readonly string _specialCharSlow = "$1";
        static readonly string _specialCharMedium = "$2";
        static readonly string _specialCharFast = "$3";
        static readonly string _specialCharPause = "$0";
        static readonly string _specialCharVariable = "$v";

        readonly ScriptGame _game;
        readonly string[] _visibleLines = new string[_numVisibleLines];
        readonly ScriptTextSurface[] _visibleLineSurfaces = new ScriptTextSurface[_numVisibleLines];

        DialogBoxStyle _style;
        ScriptSurface _dialogSurface;
        ScriptSurface _boxImg;
        ScriptSurface _iconsImg;
        ScriptSprite _endLinesSprite;

        Dialog _dialog;
        object _info;

        int _iconIndex;
        DialogBoxSkipMode _skipMode = DialogBoxSkipMode.None;
        DialogBoxCharDelays _charDelay = DialogBoxCharDelays.Fast;
        DialogBoxSelectedAnswer _selectedAnswer;
        Point _boxDstPosition;
        Point _questionDstPosition;
        Point _iconDstPosition;
        string[] _lines;
        int _lineIndex;
        int _visibleLineIndex;
        int _charIndex;
        bool _isFull;

        bool _skipped;
        bool _needLetterSound;
        bool _gradual;

        public DialogBoxVerticalPosition DialogPosition { get; private set; }
        public bool HasMoreLines { get { return (_lineIndex + 1) < _lines.Length; } }

        private string CurrentLine { get { return _visibleLines[_visibleLineIndex]; } }

        public DialogBox(ScriptGame game)
        {
            _game = game;
            DialogPosition = DialogBoxVerticalPosition.Auto;
        }

        public void Initialize()
        {
            var font = Fonts.GetDialogFont();
            for (int i = 0; i < _numVisibleLines; ++i)
            {
                _visibleLines[i] = string.Empty;
                _visibleLineSurfaces[i] = ScriptTextSurface.Create(
                    horizontalAlignment: TextHorizontalAlignment.Left,
                    verticalAlignment: TextVerticalAlignment.Top,
                    font: font.Id,
                    fontSize: font.Size);
            }
            _dialogSurface = ScriptSurface.Create(Core.Video.ModSize);
            _boxImg = ScriptSurface.Create("hud/dialog_box.png");
            _iconsImg = ScriptSurface.Create("hud/dialog_icons.png");
            _endLinesSprite = ScriptSprite.Create("hud/dialog_box_message_end");
            SetDialogStyle(DialogBoxStyle.Box);
        }

        public void Quit()
        {
            if (_game.IsDialogEnabled)
                ScriptMenu.Stop(this);
        }

        public void SetDialogStyle(DialogBoxStyle style)
        {
            _style = style;
            if (_style == DialogBoxStyle.Box)
                _dialogSurface.SetOpacity(216);
        }

        public bool OnDialogStarted(Dialog dialog, object info)
        {
            _dialog = dialog;
            _info = info;
            ScriptMenu.Start(_game, this);
            return true;
        }

        public void OnDialogFinished(Dialog dialog)
        {
            ScriptMenu.Stop(this);
            _dialog = null;
            _info = null;
        }

        protected override void OnStarted()
        {
            _iconIndex = -1;
            _skipMode = DialogBoxSkipMode.None;
            _charDelay = DialogBoxCharDelays.Fast;
            _selectedAnswer = DialogBoxSelectedAnswer.NoQuestion;

            var map = _game.Map;
            var cameraPosition = map.CameraPosition;
            var top = false;
            if (DialogPosition == DialogBoxVerticalPosition.Top)
                top = true;
            else if (DialogPosition == DialogBoxVerticalPosition.Auto)
            {
                var heroPosition = map.GetEntity<ScriptHero>("hero").Position;
                if (heroPosition.Y >= cameraPosition.Y + (cameraPosition.Height / 2 + 10))
                    top = true;
            }

            var x = cameraPosition.Width / 2 - 110;
            var y = (top) ? 32 : (cameraPosition.Height - 96);

            if (_style == DialogBoxStyle.Empty)
                y += (top) ? -24 : 24;

            _boxDstPosition = new Point(x, y);
            _questionDstPosition = new Point(x + 18, y + 27);
            _iconDstPosition = new Point(x + 18, y + 22);

            ShowDialog();
        }

        void ShowDialog()
        {
            var text = _dialog.Text;
            if (_info != null)
                text = text.Replace(_specialCharVariable, _info.ToString());

            _lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            _lineIndex = 0;

            _visibleLineIndex = 0;
            _charIndex = 0;
            _skipped = false;
            _isFull = false;
            _needLetterSound = (_style != DialogBoxStyle.Empty);

            if (_dialog.HasProperty("Skip"))
            {
                var value = _dialog.GetProperty("Skip");
                if (!Enum.TryParse<DialogBoxSkipMode>(value, out _skipMode))
                    throw new Exception("Invalid value for dialog 'skip' property: {0}".F(value));
            }

            if (_dialog.HasProperty("Icon"))
            {
                var value =_dialog.GetProperty("Icon");
                if (!int.TryParse(value, out _iconIndex))
                    throw new Exception("Invalid value for dialog 'icon' property: {0}".F(value));
            }

            if (_dialog.HasProperty("Question") && _dialog.GetProperty("Question") == "1")
                _selectedAnswer = DialogBoxSelectedAnswer.One;

            ShowMoreLines();
        }

        void ShowMoreLines()
        {
            _gradual = true;

            if (!HasMoreLines)
            {
                ShowNextDialog();
                return;
            }

            for (int i = 0; i < _numVisibleLines; ++i)
            {
                _visibleLineSurfaces[i].SetText(string.Empty);
                if (HasMoreLines)
                    _visibleLines[i] = _lines[_lineIndex++];
                else
                    _visibleLines[i] = string.Empty;
            }
            _visibleLineIndex = 0;
            _charIndex = 0;

            if (_gradual)
                ScriptTimer.Start(this, (uint)_charDelay, (Action)RepeatShowCharacter);
        }

        void RepeatShowCharacter()
        {
            CheckFull();
            while (!_isFull && CurrentLineIsFinished())
            {
                _charIndex = 0;
                ++_visibleLineIndex;
                CheckFull();
            }

            if (!_isFull)
                AddCharacter();
            else
            {
                Core.Audio.PlaySound("message_end");
                if (HasMoreLines || _dialog.HasProperty("Next") || _selectedAnswer != DialogBoxSelectedAnswer.NoQuestion)
                    _endLinesSprite.SetAnimation("next");
                else
                    _endLinesSprite.SetAnimation("last");
            }
        }

        bool CurrentLineIsFinished()
        {
            return _charIndex > CurrentLine.Length - 1;
        }

        void CheckFull()
        {
            if (_visibleLineIndex >= _numVisibleLines - 1 &&
                _charIndex > _visibleLines[_numVisibleLines - 1].Length - 1)
                _isFull = true;
            else
                _isFull = false;
        }

        void AddCharacter()
        {
            var currentChar = CurrentLine[_charIndex++];
            var textSurface = _visibleLineSurfaces[_visibleLineIndex];

            uint additionalDelay = 0;
            var special = false;
            if (currentChar == '$')
            {
                special = true;
                currentChar = CurrentLine[_charIndex++];
                
                var word = "${0}".F(currentChar);
                if (word == _specialCharPause)
                    additionalDelay = 1000;
                else if (word == _specialCharSlow)
                    additionalDelay = (uint)DialogBoxCharDelays.Slow;
                else if (word == _specialCharMedium)
                    additionalDelay = (uint)DialogBoxCharDelays.Medium;
                else if (word == _specialCharFast)
                    additionalDelay = (uint)DialogBoxCharDelays.Fast;
                else
                {
                    textSurface.SetText(textSurface.Text + "$");
                    special = false;
                }
            }

            if (!special)
            {
                textSurface.SetText(textSurface.Text + currentChar);
                if (currentChar == ' ')
                    additionalDelay -= (uint)_charDelay;
            }

            if (!special && _needLetterSound)
            {
                Core.Audio.PlaySound("message_letter");
                _needLetterSound = false;
                ScriptTimer.Start(this, _letterSoundDelay, () => _needLetterSound = true);
            }

            if (_gradual)
                ScriptTimer.Start(this, (uint)_charDelay + additionalDelay, (Action)RepeatShowCharacter);
        }

        void ShowNextDialog()
        {
            string nextDialogId;
            if (_selectedAnswer != DialogBoxSelectedAnswer.Two)
                nextDialogId = _dialog.HasProperty("Next") ? _dialog.GetProperty("Next") : null;
            else
                nextDialogId = _dialog.HasProperty("Next2") ? _dialog.GetProperty("Next2") : null;

            if (nextDialogId != null && nextDialogId != "_unknown")
            {
                _selectedAnswer = DialogBoxSelectedAnswer.NoQuestion;
                _dialog = ScriptLanguage.GetDialog(nextDialogId);
                ShowDialog();
            }
            else
            {
                object status = _selectedAnswer;

                if (_dialog.Id == "_shop.question")
                    status = _selectedAnswer == DialogBoxSelectedAnswer.One;

                _game.StopDialog(status);
            }
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            _dialogSurface.Clear();

            if (_style == DialogBoxStyle.Empty)
                dstSurface.FillColor(Color.Black, new Rectangle(_boxDstPosition, _boxSize));
            else
                _boxImg.DrawRegion(new Rectangle(_boxSize), _dialogSurface, _boxDstPosition);

            var x = _boxDstPosition.X;
            var y = _boxDstPosition.Y;
            var textX = x + ((_iconIndex == -1) ? 16 : 48);
            var textY = y - 1;
            for (int i = 0; i < _numVisibleLines; ++i)
            {
                textY += 13;
                if (_selectedAnswer != DialogBoxSelectedAnswer.NoQuestion &&
                    i == _numVisibleLines - 2 &&
                    !HasMoreLines)
                {
                    // 마지막 2줄은 "질문"입니다
                    textX += 24;
                }
                _visibleLineSurfaces[i].Draw(_dialogSurface, textX, textY);
            }

            if (_iconIndex != -1)
            {
                var row = _iconIndex / 10;
                var column = _iconIndex % 10;
                _iconsImg.DrawRegion(new Rectangle(16 * column, 16 * row, 16, 16), _dialogSurface, _iconDstPosition);
                _questionDstPosition.X = x + 50;
            }
            else
                _questionDstPosition.X = x + 18;

            if (_selectedAnswer != DialogBoxSelectedAnswer.NoQuestion && 
                _isFull &&
                !HasMoreLines)
            {
                _boxImg.DrawRegion(new Rectangle(96, 60, 8, 8), _dialogSurface, _questionDstPosition);
            }

            if (_isFull)
                _endLinesSprite.Draw(_dialogSurface, x + 103, y + 56);

            _dialogSurface.Draw(dstSurface);
        }

        protected override void OnFinished()
        {
            // TODO
        }

        protected override bool OnCommandPressed(GameCommand command)
        {
            if (command == GameCommand.Action)
            {
                if (_isFull)
                    ShowMoreLines();
                else if (_skipMode != DialogBoxSkipMode.None)
                    ShowAllNow();
            }
            else if (command == GameCommand.Attack)
            {
                if (_skipMode == DialogBoxSkipMode.All)
                {
                    _skipped = true;
                    _game.StopDialog("skipped");
                }
                else if (_isFull)
                    ShowMoreLines();
                else if (_skipMode == DialogBoxSkipMode.Current)
                    ShowAllNow();
            }
            else if (command == GameCommand.Up || command == GameCommand.Down)
            {
                if (_selectedAnswer != DialogBoxSelectedAnswer.NoQuestion &&
                    !HasMoreLines &&
                    _isFull)
                {
                    Core.Audio.PlaySound("cursor");
                    _selectedAnswer = (DialogBoxSelectedAnswer)(3 - (int)_selectedAnswer);
                    var yOffset = (_selectedAnswer == DialogBoxSelectedAnswer.One) ? 27 : 40;
                    _questionDstPosition.Y = _boxDstPosition.Y + yOffset;
                }
            }

            return true;
        }

        void ShowAllNow()
        {
            if (_isFull)
            {
                ShowMoreLines();
                return;
            }

            _gradual = false;
            CheckFull();
            while (!_isFull)
            {
                while (!_isFull && CurrentLineIsFinished())
                {
                    _charIndex = 0;
                    ++_visibleLineIndex;
                    CheckFull();
                }

                if (!_isFull)
                    AddCharacter();
                CheckFull();
            }
        }
    }
}
