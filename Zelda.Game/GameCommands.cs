using System;
using System.Collections.Generic;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    // 인게임 하이레벨 커맨드들과 그들의 키보드나 조이패드 매핑을 저장합니다.
    // 이 클래스는 키보드나 조이패드의 로우 레벨 이벤트를 받아 내장된 게임 커맨드들이 눌렸거나 릴리즈되었음을
    // 적당한 객체에게 알려주는 역할을 합니다.
    class GameCommands
    {
        readonly Game _game;
   
        static readonly ushort[] _directionMasks = new ushort[]
        {
            0x0001,
            0x0002,
            0x0004,
            0x0008
        };

        static readonly int[] _masksToDirection8 = new int[]
        {
            -1, // none: stop
             0, // right
             2, // up
             1, // right + up
             4, // left
            -1, // left + right: stop
             3, // left + up
            -1, // left + right + up: stop
             6, // down
             7, // down + right
            -1, // down + up: stop
            -1, // down + right + up: stop
             5, // down + left
            -1, // down + left + right: stop
            -1, // down + left + up: stop
            -1, // down + left + right + up: stop
        };

        public GameCommands(Game game)
        {
            _game = game;

            foreach (GameCommand command in Enum.GetValues(typeof(GameCommand)))
            {
                if (command == GameCommand.None)
                    continue;

                InputEvent.KeyboardKeys keyboardKey = GetSavedKeyboardBinding(command);
                _keyboardMapping[keyboardKey] = command;
            }
        }

        Savegame Savegame
        {
            get { return _game.SaveGame; }
        }

        InputEvent.KeyboardKeys GetSavedKeyboardBinding(GameCommand command)
        {
            Savegame.Key savegameKey = GetKeyboardBindingSavegameKey(command);
            string keyboardKeyName = Savegame.GetString(savegameKey);
            return InputEvent.GetKeyboardKeyByName(keyboardKeyName);
        }

        readonly static Dictionary<GameCommand, Savegame.Key> _savegameKeys = new Dictionary<GameCommand, Savegame.Key>()
        {
            { GameCommand.Action, Savegame.Key.KeyboardAction },
            { GameCommand.Attack, Savegame.Key.KeyboardAttack },
            { GameCommand.Item1, Savegame.Key.KeyboardItem1 },
            { GameCommand.Item2, Savegame.Key.KeyboardItem2 },
            { GameCommand.Pause, Savegame.Key.KeyboardPause },
            { GameCommand.Right, Savegame.Key.KeyboardRight },
            { GameCommand.Up, Savegame.Key.KeyboardUp },
            { GameCommand.Left, Savegame.Key.KeyboardLeft },
            { GameCommand.Down, Savegame.Key.KeyboardDown }
        };

        Savegame.Key GetKeyboardBindingSavegameKey(GameCommand command)
        {
            return _savegameKeys[command];
        }

        public void NotifyInput(InputEvent inputEvent)
        {
            if (inputEvent.IsKeyboardKeyPressed)
                KeyboardKeyPressed(inputEvent.KeyboardKey);
            else if (inputEvent.IsKeyboardKeyReleased)
                KeyboardKeyReleased(inputEvent.KeyboardKey);
        }

        public int GetWantedDirection8()
        {
            ushort directionMask = 0x0000;
            if (IsCommandPressed(GameCommand.Right))
                directionMask |= _directionMasks[0];
            if (IsCommandPressed(GameCommand.Up))
                directionMask |= _directionMasks[1];
            if (IsCommandPressed(GameCommand.Left))
                directionMask |= _directionMasks[2];
            if (IsCommandPressed(GameCommand.Down))
                directionMask |= _directionMasks[3];

            return _masksToDirection8[directionMask];
        }

        public bool IsCommandPressed(GameCommand command)
        {
            return _commandsPressed.Contains(command);
        }

        #region 키보드 매핑
        readonly Dictionary<InputEvent.KeyboardKeys, GameCommand> _keyboardMapping = new Dictionary<InputEvent.KeyboardKeys,GameCommand>();

        void KeyboardKeyPressed(InputEvent.KeyboardKeys keyboardKeyPressed)
        {
            GameCommand commandPressed = GetCommandFromKeyboard(keyboardKeyPressed);
            if (commandPressed != GameCommand.None)
                GameCommandPressed(commandPressed);
        }

        void KeyboardKeyReleased(InputEvent.KeyboardKeys keyboardKeyReleased)
        {
            GameCommand commandReleased = GetCommandFromKeyboard(keyboardKeyReleased);
            if (commandReleased != GameCommand.None)
                GameCommandReleased(commandReleased);
        }

        GameCommand GetCommandFromKeyboard(InputEvent.KeyboardKeys key)
        {
            GameCommand command;
            if (_keyboardMapping.TryGetValue(key, out command))
                return command;
            
            return GameCommand.None;
        }
        #endregion

        #region 하이레벨 커맨드 처리
        readonly HashSet<GameCommand> _commandsPressed = new HashSet<GameCommand>();
        
        void GameCommandPressed(GameCommand command)
        {
            _commandsPressed.Add(command);
            _game.NotifyCommandPressed(command);
        }

        void GameCommandReleased(GameCommand command)
        {
            _commandsPressed.Remove(command);
        }
        #endregion
    }
}
