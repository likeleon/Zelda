using System;
using System.Collections.Generic;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game
{
    class DialogBoxSystem
    {
        readonly Game _game;
        public Game Game
        {
            get { return _game; }
        }

        string _dialogId;
        public string DialogId
        {
            get { return _dialogId; }
        }

        public bool IsEnabled
        {
            get { return !String.IsNullOrEmpty(_dialogId); }
        }

        Dialog _dialog;
        Action _callback;
        bool _builtIn;
        readonly Queue<string> _remainingLines = new Queue<string>();
        Point _textPosition;
        static readonly int _numVisibleLines = 3;

        public DialogBoxSystem(Game game)
        {
            _game = game;
        }

        public void Open(string dialogId, Action callback)
        {
            Debug.CheckAssertion(!IsEnabled, "A dialog is already active");

            _dialogId = dialogId;
            _dialog = DialogResource.GetDialog(dialogId);
            _callback = callback;

            CommandsEffects commandsEffects = _game.CommandsEffects;
            commandsEffects.SaveActionCommandEffect();
            commandsEffects.ActionCommandEffect = ActionCommandEffect.None;

            _builtIn = true;    // TODO: 스크립트에서 커스텀 버전 쓸 수 있게 지원
            if (_builtIn)
            {
                // 내장 대화창을 보여주는 경우입니다
                commandsEffects.ActionCommandEffect = ActionCommandEffect.Next;

                // 텍스트를 준비합니다
                string text = _dialog.Text;

                if (dialogId == "_shop.question")
                    throw new NotImplementedException("Shop Dialog");

                _remainingLines.Clear();
                foreach (string line in text.Split('\n'))
                    _remainingLines.Enqueue(line);

                // 위치를 결정합니다
                Rectangle cameraPosition = _game.CurrentMap.CameraPosition;
                bool top = (_game.Hero.Y >= cameraPosition.Y + 130);
                int x = cameraPosition.Width / 2 - 110;
                int y = top ? 32 : cameraPosition.Height - 96;

                _textPosition = new Point(x, y);

                // 텍스트를 보여주기 시작합니다
                ShowMoreLines();
            }
        }

        public void Close()
        {
            Debug.CheckAssertion(IsEnabled, "No dialog is active");

            Action callback = _callback;
            _callback = null;
            _dialogId = String.Empty;

            CommandsEffects commandsEffects = _game.CommandsEffects;
            commandsEffects.RestoreActionCommandEffect();

            ScriptContext.NotifyDialogFinished(_game, _dialog, callback);
        }

        void ShowMoreLines()
        {
            Debug.CheckAssertion(_builtIn, "This dialog box is not the built-in one");

            if (!HasMoreLines)
            {
                Close();
                return;
            }

            CommandsEffects commandsEffects = _game.CommandsEffects;
            commandsEffects.ActionCommandEffect = ActionCommandEffect.Next;

            // 다음 3라인을 준비합니다
            int textX = _textPosition.X;
            int textY = _textPosition.Y;
            for (int i = 0; i < _numVisibleLines; ++i)
            {
                textY += 16;

                if (HasMoreLines)
                {
                    Console.WriteLine("Dialog line {0}: {1}".F(i, _remainingLines.Peek()));
                    _remainingLines.Dequeue();
                }
            }
        }

        bool HasMoreLines
        {
            get { return _remainingLines.Count > 0; }
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
    }
}
