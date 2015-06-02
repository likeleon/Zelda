using System;
using System.Linq;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus
{
    class LanguageMenu : ScriptMenu
    {
        sealed class Language
        {
            readonly string _id;
            readonly ScriptTextSurface _text;

            public string Id { get { return _id; } }
            public ScriptTextSurface Text { get { return _text; } }
            public int Y { get; set; }

            public Language(string id, string font, int fontSize)
            {
                _id = id;
                _text = ScriptTextSurface.Create(
                    font: font, 
                    fontSize: fontSize, 
                    text: ScriptLanguage.GetLanguageName(id),
                    horizontalAlignment: TextHorizontalAlignment.Center);
            }
        }

        ScriptSurface _surface;
        bool _finished;
        int _cursorPosition = -1;
        int _firstVisibleLanguage = 0;
        int _maxVisibleLanguages = 10;
        int _numVisibleLanguages;
        Language[] _languages;

        protected override void OnStarted()
        {
            if (!String.IsNullOrEmpty(ScriptLanguage.Language))
            {
                ScriptMenu.Stop(this);
                return;
            }

            _surface = ScriptSurface.Create(320, 240);
            _numVisibleLanguages = Math.Min(ScriptLanguage.Languages.Count(), _maxVisibleLanguages);

            _languages = new Language[ScriptLanguage.Languages.Count()];

            var defaultId = "en";
            var index = 0;
            var cursorPosition = 0;
            foreach (var id in ScriptLanguage.Languages)
            {
                var font = LanguageFonts.GetMenuFont(id);
                Language language = new Language(id, font.Item1, font.Item2);

                if (id == defaultId)
                    cursorPosition = index;

                _languages[index] = language;
                ++index;
            }

            if (_languages.Length <= 1)
            {
                if (_languages.Length == 1)
                    ScriptLanguage.SetLanguage(_languages[0].Id);
                ScriptMenu.Stop(this);
            }
            else
                SetCursorPosition(cursorPosition);
        }

        void SetCursorPosition(int cursorPosition)
        {
            if (_cursorPosition != -1)
                _languages[_cursorPosition].Text.SetColor(Color.White);
            _languages[cursorPosition].Text.SetColor(Color.Yellow);

            if (cursorPosition < _firstVisibleLanguage)
                _firstVisibleLanguage = cursorPosition;

            if (cursorPosition >= _firstVisibleLanguage + _maxVisibleLanguages)
                _firstVisibleLanguage = cursorPosition - _maxVisibleLanguages;

            _cursorPosition = cursorPosition;
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            _surface.Clear();

            var y = 120 - 8 * _numVisibleLanguages;
            for (var i = _firstVisibleLanguage; i < _firstVisibleLanguage + _numVisibleLanguages; ++i)
            {
                _languages[i].Y = y;
                y += 16;
                _languages[i].Text.Draw(_surface, 160, y);
            }

            _surface.Draw(dstSurface, dstSurface.Width / 2 - 160, dstSurface.Height / 2 - 120);
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            bool handled = false;

            if (key == KeyboardKey.KEY_ESCAPE)
            {
                handled = true;
                ScriptMain.Exit();
            }
            else if (key == KeyboardKey.KEY_SPACE || key == KeyboardKey.KEY_RETURN)
            {
                if (!_finished)
                {
                    handled = true;
                    var language = _languages[_cursorPosition];
                    ScriptLanguage.SetLanguage(language.Id);
                    _finished = true;
                    _surface.FadeOut();
                    ScriptTimer.Start(this, 700, (Action)Stop);
                }
            }
            else if (key == KeyboardKey.KEY_RIGHT)
                handled = DirectionPressed(Direction8.Right);
            else if (key == KeyboardKey.KEY_UP)
                handled = DirectionPressed(Direction8.Up);
            else if (key == KeyboardKey.KEY_LEFT)
                handled = DirectionPressed(Direction8.Left);
            else if (key == KeyboardKey.KEY_DOWN)
                handled = DirectionPressed(Direction8.Down);

            return handled;
        }

        bool DirectionPressed(Direction8 direction)
        {
            if (_finished)
                return false;

            var n = _languages.Length;
            if (direction == Direction8.Up)
            {
                ScriptAudio.PlaySound("cursor");
                SetCursorPosition((_cursorPosition + n - 1) % n);
                return true;
            }
            else if (direction == Direction8.Down)
            {
                ScriptAudio.PlaySound("cursor");
                SetCursorPosition((_cursorPosition + 1) % n);
                return true;
            }
            else
                return false;
        }
    }
}
