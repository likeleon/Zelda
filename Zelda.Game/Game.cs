using Zelda.Game.Engine;
using System;
using System.Linq;
using Zelda.Game.Entities;
using Key = Zelda.Game.Savegame.Key;

namespace Zelda.Game
{
    class Game
    {
        bool _started;

        public Game(MainLoop mainLoop, Savegame saveGame)
        {
            _mainLoop = mainLoop;
            _saveGame = saveGame;

            _saveGame.Game = this;

            // 멤버 초기화
            _commands = new GameCommands(this);
            _hero = new Hero(Equipment);
            UpdateCommandsEffects();
            
            // 게임 오버 이후에 재시작하는 경우에 대한 처리입니다
            if (Equipment.Life <= 0)
                Equipment.RestoreAllLife();

            // 시작 맵을 시작합니다
            string startingMapId = _saveGame.GetString(Key.StartingMap);
            string startingDestinationName = _saveGame.GetString(Key.StartingPoint);

            bool validMapSaved = false;
            if (!String.IsNullOrEmpty(startingMapId))
            {
                if (CurrentMod.ResourceExists(ResourceType.Map, startingMapId))
                    validMapSaved = true;
                else
                {
                    // 더 이상 존재하지 않는 맵을 사용하려고 합니다. 
                    // 개발 중에 나타날 수 있는 일로 에러를 보여주고 기본 맵을 사용합니다.
                    Debug.Error("The savegame referes to a non-existing map: '{0}'".F(startingMapId));
                }
            }

            if (!validMapSaved)
            {
                // 유효한 시작 맵이 없을 경우 리소스 목록에서 첫번째 맵을 사용합니다
                var maps = CurrentMod.GetResources(ResourceType.Map);
                if (maps.Count <= 0)
                    Debug.Die("This quest has no map");

                startingMapId = maps.First().Key;
                startingDestinationName = String.Empty;
            }

            SetCurrentMap(startingMapId, startingDestinationName);
        }

        public void Start()
        {
            if (_started)
                return;

            _started = true;
        }

        public void Stop()
        {
            if (!_started)
                return;

            _saveGame.Game = null;

            _started = false;
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

        #region 전역 객체들
        readonly MainLoop _mainLoop;
        public MainLoop MainLoop
        {
            get { return _mainLoop; }
        }

        public Equipment Equipment
        {
            get { return _saveGame.Equipment; }
        }

        readonly Savegame _saveGame;
        public Savegame SaveGame
        {
            get { return _saveGame; }
        }

        readonly Hero _hero;
        public Hero Hero
        {
            get { return _hero; }
        }

        readonly GameCommands _commands;
        public GameCommands Commands
        {
            get { return _commands; }
        }

        readonly CommandsEffects _commandsEffects = new CommandsEffects();
        public CommandsEffects CommandsEffects
        {
            get { return _commandsEffects; }
        }
        #endregion

        #region 메인 루프에 의해 호출되는 함수들
        public bool NotifyInput(InputEvent inputEvent)
        {
            if (_currentMap != null && _currentMap.IsLoaded)
                _commands.NotifyInput(inputEvent);
            return true;
        }

        public void Update()
        {
            UpdateTransitions();

            if (!_started)
                return;

            _currentMap.Update();

            Equipment.Update();
            UpdateCommandsEffects();
        }

        public void Draw(Surface dstSurface)
        {
            if (_currentMap == null)
                return; // 게임의 초기화가 완료되기 전입니다

            if (_currentMap.IsLoaded)
            {
                _currentMap.Draw();
                _currentMap.VisibleSurface.Draw(dstSurface);
            }
        }
        #endregion

        #region 현재 게임 상태
        bool _paused;
        public bool IsPaused
        {
            get { return _paused; }
        }

        public bool IsSuspended
        {
            get
            {
                return _currentMap == null ||
                       IsPaused;
            }
        }
        #endregion
        
        #region 일시 정지
        public bool CanPause
        {
            get
            {
                return !IsSuspended &&
                       Equipment.Life > 0;  // 게임 오버 처리가 시작되려고 할 때에는 일시 정지를 허용하면 안 됩니다
            }
        }

        public bool CanUnpause
        {
            get
            {
                return IsPaused;
            }
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused)
                return;

            _paused = paused;
            if (paused)
            {
                _commandsEffects.SaveActionCommandEffect();
                _commandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
            else
            {
                _commandsEffects.RestoreActionCommandEffect();
            }
        }
        #endregion

        #region 게임 컨트롤
        public void NotifyCommandPressed(GameCommand command)
        {
            if (command == GameCommand.Pause)
            {
                if (IsPaused)
                {
                    if (CanUnpause)
                        SetPaused(false);
                }
                else
                {
                    if (CanPause)
                        SetPaused(true);
                }
            }
        }
        #endregion

        #region 맵
        Map _currentMap;
        public bool HasCurrentMap
        {
            get { return _currentMap != null; }
        }

        public Map CurrentMap
        {
            get { return _currentMap; }
        }

        Map _nextMap;

        public void SetCurrentMap(string mapId, string destinationName)
        {
            // 다음 맵을 준비합니다
            if (_currentMap == null || mapId != _currentMap.Id)
            {
                // 다른 맵입니다
                _nextMap = new Map(mapId);
                _nextMap.Load(this);
            }
            else
            {
                // 동일한 맵입니다
                _nextMap = _currentMap;
            }

            _nextMap.DestinationName = destinationName;
        }
        #endregion

        #region 갱신 함수들
        void UpdateCommandsEffects()
        {
        }

        void UpdateTransitions()
        {
            if (_nextMap != null)
            {
                if (_currentMap == null)
                {
                    _currentMap = _nextMap;
                    _nextMap = null;
                }
            }

            Rectangle previousMapLocation = _currentMap.Location;

            if (_started && !_currentMap.IsStarted)
            {
                Debug.CheckAssertion(_currentMap.IsLoaded, "This map is not loaded");
                _hero.PlaceOnDestination(_currentMap, previousMapLocation);
                _currentMap.Start();
            }
        }
        #endregion
    }
}
