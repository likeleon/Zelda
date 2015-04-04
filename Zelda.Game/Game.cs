using Zelda.Game.Engine;
using System;
using System.Linq;

namespace Zelda.Game
{
    class Game
    {
        readonly MainLoop _mainLoop;
        public MainLoop MainLoop
        {
            get { return _mainLoop; }
        }

        public Equipment Equipment
        {
            get { return _saveGame.Equipment; }
        }

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

        readonly SaveGame _saveGame;
        bool _started;

        public Game(MainLoop mainLoop, SaveGame saveGame)
        {
            _mainLoop = mainLoop;
            _saveGame = saveGame;

            _saveGame.Game = this;
            
            // 게임 오버 이후에 재시작하는 경우에 대한 처리입니다
            if (Equipment.Life <= 0)
                Equipment.RestoreAllLife();

            // 시작 맵을 시작합니다
            string startingMapId = _saveGame.GetString(SaveGame.Key.StartingMap);
            string startingDestinationName = _saveGame.GetString(SaveGame.Key.StartingPoint);

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

        public bool NotifyInput(InputEvent inputEvent)
        {
            return true;
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

        public void Update()
        {
            UpdateTransitions();
            
            if (!_started)
                return;
        }

        private void UpdateTransitions()
        {
            if (_nextMap != null)
            {
                if (_currentMap == null)
                {
                    _currentMap = _nextMap;
                    _nextMap = null;
                }
            }

            if (_started && !_currentMap.IsStarted)
            {
                Debug.CheckAssertion(_currentMap.IsLoaded, "This map is not loaded");
                _currentMap.Start();
            }
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
    }
}
