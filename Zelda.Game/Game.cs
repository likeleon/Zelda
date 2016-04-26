using System;
using System.Linq;
using Zelda.Game.Lowlevel;
using Zelda.Game.Entities;
using Zelda.Game.Script;
using Key = Zelda.Game.Savegame.Key;

namespace Zelda.Game
{
    class Game
    {
        readonly DialogBoxSystem _dialogBox;
        bool _started;
        Map _nextMap;

        public Framework Framework { get; }
        public Equipment Equipment { get { return SaveGame.Equipment; } }
        public Savegame SaveGame { get; }
        public Hero Hero { get; }
        public GameCommands Commands { get; }
        public CommandsEffects CommandsEffects { get; } = new CommandsEffects();

        public bool IsPaused { get; private set; }
        public bool IsDialogEnabled { get { return _dialogBox.IsEnabled; } }
        public bool IsSuspended { get { return CurrentMap == null || IsPaused; } }

        public bool CanPause { get { return !IsSuspended && Equipment.GetLife() > 0; } } // 게임 오버 처리가 시작되려고 할 때에는 일시 정지를 허용하면 안 됩니다
        public bool CanUnpause { get { return IsPaused; } }

        public bool HasCurrentMap { get { return CurrentMap != null; } }
        public Map CurrentMap { get; private set; }

        public Game(Framework framework, Savegame saveGame)
        {
            Framework = framework;
            SaveGame = saveGame;
            _dialogBox = new DialogBoxSystem(this);

            SaveGame.Game = this;

            // 멤버 초기화
            Commands = new GameCommands(this);
            Hero = new Hero(Equipment);
            UpdateCommandsEffects();
            
            // 게임 오버 이후에 재시작하는 경우에 대한 처리입니다
            if (Equipment.GetLife() <= 0)
                Equipment.RestoreAllLife();

            // 시작 맵을 시작합니다
            string startingMapId = SaveGame.GetString(Key.StartingMap);
            string startingDestinationName = SaveGame.GetString(Key.StartingPoint);

            bool validMapSaved = false;
            if (!startingMapId.IsNullOrEmpty())
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
                    Debug.Die("This mod has no map");

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
            SaveGame.ScriptGame.NotifyStarted();
        }

        public void Stop()
        {
            if (!_started)
                return;

            if (CurrentMap != null)
            {
                if (Hero.IsOnMap)
                    Hero.NotifyBeingRemoved();
            }

            SaveGame.ScriptGame.NotifyFinished();
            SaveGame.NotifyGameFinished();
            SaveGame.Game = null;

            _started = false;
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

        public bool NotifyInput(InputEvent inputEvent)
        {
            if (CurrentMap != null && CurrentMap.IsLoaded)
                Commands.NotifyInput(inputEvent);
            return true;
        }

        public void Update()
        {
            UpdateTransitions();

            if (!_started)
                return;

            CurrentMap.Update();

            Equipment.Update();
            UpdateCommandsEffects();
        }

        public void Draw(Surface dstSurface)
        {
            if (CurrentMap == null)
                return; // 게임의 초기화가 완료되기 전입니다

            if (CurrentMap.IsLoaded)
            {
                CurrentMap.Draw();
                CurrentMap.VisibleSurface.Draw(dstSurface);

                if (IsDialogEnabled)
                    _dialogBox.Draw(dstSurface);
            }

            ScriptContext.GameOnDraw(this, dstSurface.ScriptSurface);
        }

        public bool NotifyDialogStarted(Dialog dialog, object info)
        {
            return SaveGame.ScriptGame.NotifyDialogStarted(dialog, info);
        }

        public void NotifyDialogFinished(Dialog dialog, Action<object> callback, object status)
        {
            SaveGame.ScriptGame.NotifyDialogFinished(dialog);

            if (callback != null)
                CoreToScript.Call(() => callback(status));
        }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;

            IsPaused = paused;
            if (paused)
            {
                CommandsEffects.SaveActionCommandEffect();
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
            else
            {
                CommandsEffects.RestoreActionCommandEffect();
            }
        }

        public void StartDialog(string dialogId, object info, Action<object> callback)
        {
            if (!CurrentMod.DialogExists(dialogId))
                Debug.Error("No such dialog: '{0}'".F(dialogId));
            else
                _dialogBox.Open(dialogId, info, callback);
        }

        public void StopDialog(object status)
        {
            _dialogBox.Close(status);
        }

        public void NotifyCommandPressed(GameCommand command)
        {
            if (IsDialogEnabled)
            {
                if (_dialogBox.NotifyCommandPressed(command))
                    return;
            }

            if (SaveGame.ScriptGame.NotifyCommandPressed(command))
                return;

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
            else if (!IsSuspended)
                Hero.NotifyCommandPressed(command);
        }

        public void SetCurrentMap(string mapId, string destinationName)
        {
            // 다음 맵을 준비합니다
            if (CurrentMap == null || mapId != CurrentMap.Id)
            {
                // 다른 맵입니다
                _nextMap = new Map(mapId);
                _nextMap.Load(this);
            }
            else
            {
                // 동일한 맵입니다
                _nextMap = CurrentMap;
            }

            _nextMap.DestinationName = destinationName;
        }

        void UpdateCommandsEffects()
        {
        }

        void UpdateTransitions()
        {
            if (_nextMap != null)
            {
                if (CurrentMap == null)
                {
                    CurrentMap = _nextMap;
                    _nextMap = null;
                }
            }

            Rectangle previousMapLocation = CurrentMap.Location;

            if (_started && !CurrentMap.IsStarted)
            {
                Debug.CheckAssertion(CurrentMap.IsLoaded, "This map is not loaded");
                Hero.PlaceOnDestination(CurrentMap, previousMapLocation);
                CurrentMap.Start();
            }
        }
    }
}
