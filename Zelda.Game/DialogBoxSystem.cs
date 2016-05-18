using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    class DialogBoxSystem
    {
        static readonly int _numVisibleLines = 3;

        readonly Queue<string> _remainingLines = new Queue<string>();
        readonly TextSurface[] _lineSurfaces = new TextSurface[_numVisibleLines];

        Dialog _dialog;
        Action<object> _callback;
        bool _builtIn;
        Point _textPosition;

        public Game Game { get; }
        public string DialogId { get; private set; }
        public bool IsEnabled { get { return !DialogId.IsNullOrEmpty(); } }
        bool HasMoreLines { get { return _remainingLines.Count > 0; } }

        public DialogBoxSystem(Game game)
        {
            Game = game;

            for (int i = 0; i < _numVisibleLines; ++i)
                _lineSurfaces[i] = new TextSurface(0, 0, TextHorizontalAlignment.Left, TextVerticalAlignment.Bottom);
        }

        public void Open(string dialogId, object info, Action<object> callback)
        {
            Debug.CheckAssertion(!IsEnabled, "A dialog is already active");

            DialogId = dialogId;
            _dialog = Core.Mod.GetDialog(dialogId);
            _callback = callback;

            var commandsEffects = Game.CommandsEffects;
            commandsEffects.SaveActionCommandEffect();
            commandsEffects.ActionCommandEffect = ActionCommandEffect.None;

            _builtIn = !Game.NotifyDialogStarted(_dialog, info);
            if (_builtIn)
            {
                // 내장 대화창을 보여주는 경우입니다
                commandsEffects.ActionCommandEffect = ActionCommandEffect.Next;

                // 텍스트를 준비합니다
                string text = _dialog.Text;

                if (dialogId == "_shop.question")
                    throw new NotImplementedException("Shop Dialog");

                _remainingLines.Clear();
                text.Split('\n').Do(line => _remainingLines.Enqueue(line));

                // 위치를 결정합니다
                var cameraPosition = Game.CurrentMap.CameraPosition;
                bool top = (Game.Hero.Y >= cameraPosition.Y + 130);
                int x = cameraPosition.Width / 2 - 110;
                int y = top ? 32 : cameraPosition.Height - 96;

                _textPosition = new Point(x, y);

                // 텍스트를 보여주기 시작합니다
                ShowMoreLines();
            }
        }

        public void Close(object status)
        {
            Debug.CheckAssertion(IsEnabled, "No dialog is active");

            var callback = _callback;
            _callback = null;
            DialogId = "";

            var commandsEffects = Game.CommandsEffects;
            commandsEffects.RestoreActionCommandEffect();

            Game.NotifyDialogFinished(_dialog, callback, status);
        }

        void ShowMoreLines()
        {
            Debug.CheckAssertion(_builtIn, "This dialog box is not the built-in one");

            if (!HasMoreLines)
            {
                Close(null);
                return;
            }

            Game.CommandsEffects.ActionCommandEffect = ActionCommandEffect.Next;

            // 다음 3라인을 준비합니다
            int textX = _textPosition.X;
            int textY = _textPosition.Y;
            for (int i = 0; i < _numVisibleLines; ++i)
            {
                textY += 16;
                _lineSurfaces[i].SetPosition(textX, textY);
                _lineSurfaces[i].SetTextColor(Color.White);

                if (HasMoreLines)
                {
                    _lineSurfaces[i].SetText(_remainingLines.Peek());
                    _remainingLines.Dequeue();
                }
                else
                    _lineSurfaces[i].SetText(String.Empty);
            }
        }

        public bool NotifyCommandPressed(GameCommand command)
        {
            if (!IsEnabled)
                return false;

            if (!_builtIn)
                return false;

            if (command == GameCommand.Action)
                ShowMoreLines();

            return true;
        }

        public void Draw(Surface dstSurface)
        {
            if (!_builtIn)
                return;

            foreach (TextSurface lineSurface in _lineSurfaces)
                lineSurface.Draw(dstSurface);
        }
    }
}
