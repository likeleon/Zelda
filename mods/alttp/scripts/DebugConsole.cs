using System.Collections.Generic;
using System.Linq;
using Zelda.Game;
using Zelda.Game.Lowlevel;
using Zelda.Game.Script;

namespace Alttp
{
    class DebugConsole : ScriptMenu
    {
        static readonly int _historyCapacity = 50;
        static readonly char[] _controlChars = "!@#$%^&*()".ToCharArray();

        readonly List<string> _history = new List<string>(_historyCapacity);
        readonly Color _color = new Color(64, 64, 64);
        readonly ScriptTextSurface _inputTextSurface;
        readonly ScriptTextSurface _outputTextSurface;
        int _historyPosition;

        public bool IsEnabled { get; private set; }
        private string InputText { get { return _inputTextSurface.Text; } }
        private string OutputText { get { return _outputTextSurface.Text; } }

        public DebugConsole()
        {
            _inputTextSurface = ScriptTextSurface.Create(font: "minecraftia", fontSize: 8);
            _outputTextSurface = ScriptTextSurface.Create(font: "minecraftia", fontSize: 8);

            for (int i = 0; i < _historyCapacity; ++i)
                _history.Add(string.Empty);
        }

        protected override void OnStarted()
        {
            IsEnabled = true;
            BuildInputText();
        }

        protected override void OnFinished()
        {
            IsEnabled = false;
        }

        void BuildInputText()
        {
            var text = "> {0}".F(_history[_historyPosition]);
            _inputTextSurface.SetText(text);
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            var width = dstSurface.Width;
            var height = dstSurface.Height;
            dstSurface.FillColor(_color, new Rectangle(32, height - 64, width - 64, 40));
            _inputTextSurface.Draw(dstSurface, 40, height - 56);
            _outputTextSurface.Draw(dstSurface, 40, height - 40);
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            if (key == KeyboardKey.F12 || key == KeyboardKey.Escape)
                ScriptMenu.Stop(this);
            else if (key == KeyboardKey.Backspace)
            {
                if (OutputText.Length > 0)
                    Clear();
                else
                    RemoveInputCharacter();
            }
            else if ((key == KeyboardKey.Return || key == KeyboardKey.KpEnter) &&
                     (!modifiers.Alt && !modifiers.Control))
            {
                if (OutputText.Length > 0)
                    Clear();
                else
                    ExecuteCode();
            }
            else if (key == KeyboardKey.Up)
                HistoryUp();
            else if (key == KeyboardKey.Down)
                HistoryDown();

            return true;    // exclusive focus
        }

        void Clear()
        {
            _history[_historyPosition] = string.Empty;
            BuildInputText();
            SetOutputText(string.Empty);
        }

        void RemoveInputCharacter()
        {
            var line = _history[_historyPosition];
            if (line.Length <= 0)
                return;

            _history[_historyPosition] = line.Remove(line.Length - 1);
            BuildInputText();
        }

        void HistoryUp()
        {
            if (OutputText.Length > 0)
                Clear();

            if (_historyPosition > 0)
            {
                _historyPosition -= 1;
                BuildInputText();
            }
        }

        void HistoryDown()
        {
            if (OutputText.Length > 0)
                Clear();

            if (_historyPosition < _history.Count - 1)
            {
                _historyPosition += 1;
                BuildInputText();
            }
        }

        void SetOutputText(string text)
        {
            _outputTextSurface.SetText(text);
        }

        void SetInputText(string text)
        {
            _inputTextSurface.SetText(text);
        }

        void ExecuteCode()
        {
            HistoryAddCommand();

            // TODO: execute
            SetOutputText("TODO: execute {0}".F(InputText));
        }

        void HistoryAddCommand()
        {
            if (_history.Count >= _historyCapacity)
            {
                _history.RemoveAt(0);
                _history.Add(string.Empty);
            }

            if (_historyPosition < _history.Count - 1)
            {
                // 히스토리를 탐색 중인 상태로, 선택한 커맨드를 현재 커맨드로 저장해줍니다.
                _history[_history.Count - 1] = _history[_historyPosition];
            }
            _historyPosition = _history.Count - 1;
            _history[_historyPosition] = string.Empty;
        }

        public override bool OnCharacterPressed(string character)
        {
            if (character.IndexOfAny(_controlChars) != -1)
                return false;

            if (OutputText.Length > 0)
                Clear();
            AppendInputCharacter(character);
            return true;
        }

        void AppendInputCharacter(string character)
        {
            _history[_historyPosition] += character;
            BuildInputText();
        }
    }
}
