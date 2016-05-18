using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    // 인게임 하이레벨 커맨드들과 그들의 키보드나 조이패드 매핑을 저장합니다.
    // 이 클래스는 키보드나 조이패드의 로우 레벨 이벤트를 받아 내장된 게임 커맨드들이 눌렸거나 릴리즈되었음을
    // 적당한 객체에게 알려주는 역할을 합니다.
    class GameCommands
    {
        static readonly ushort[] _directionMasks = new ushort[]
        {
            0x0001,
            0x0002,
            0x0004,
            0x0008
        };

        static readonly Direction8[] _masksToDirection8 = new Direction8[]
        {
            Direction8.None,        // none: stop
            Direction8.Right,       // right
            Direction8.Up,          // up
            Direction8.RightUp,     // right + up
            Direction8.Left,        // left
            Direction8.None,        // left + right: stop
            Direction8.LeftUp,      // left + up
            Direction8.None,        // left + right + up: stop
            Direction8.Down,        // down
            Direction8.RightDown,   // down + right
            Direction8.None,        // down + up: stop
            Direction8.None,        // down + right + up: stop
            Direction8.LeftDown,    // down + left
            Direction8.None,        // down + left + right: stop
            Direction8.None,        // down + left + up: stop
            Direction8.None,        // down + left + right + up: stop
        };

        readonly Game _game;
        readonly HashSet<GameCommand> _commandsPressed = new HashSet<GameCommand>();
        readonly Dictionary<KeyboardKey, GameCommand> _keyboardMapping = new Dictionary<KeyboardKey, GameCommand>();
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

        Savegame Savegame { get { return _game.SaveGame; } }

        public GameCommands(Game game)
        {
            _game = game;

            foreach (GameCommand command in Enum.GetValues(typeof(GameCommand)))
            {
                if (command == GameCommand.None)
                    continue;

                var keyboardKey = GetSavedKeyboardBinding(command);
                _keyboardMapping[keyboardKey] = command;
            }
        }

        KeyboardKey GetSavedKeyboardBinding(GameCommand command)
        {
            var savegameKey = GetKeyboardBindingSavegameKey(command);
            var keyboardKeyName = Savegame.GetString(savegameKey);
            return Core.Input.GetKeyboardKeyByName(keyboardKeyName);
        }

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

        public Direction8 GetWantedDirection8()
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

        void KeyboardKeyPressed(KeyboardKey keyboardKeyPressed)
        {
            GameCommand commandPressed = GetCommandFromKeyboard(keyboardKeyPressed);
            if (commandPressed != GameCommand.None)
                GameCommandPressed(commandPressed);
        }

        void KeyboardKeyReleased(KeyboardKey keyboardKeyReleased)
        {
            GameCommand commandReleased = GetCommandFromKeyboard(keyboardKeyReleased);
            if (commandReleased != GameCommand.None)
                GameCommandReleased(commandReleased);
        }

        GameCommand GetCommandFromKeyboard(KeyboardKey key)
        {
            GameCommand command;
            if (_keyboardMapping.TryGetValue(key, out command))
                return command;
            
            return GameCommand.None;
        }

        void GameCommandPressed(GameCommand command)
        {
            _commandsPressed.Add(command);
            _game.NotifyCommandPressed(command);
        }

        void GameCommandReleased(GameCommand command)
        {
            _commandsPressed.Remove(command);
        }
    }
}
