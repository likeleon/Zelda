using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;
using Key = Zelda.Game.Savegame.Key;

namespace Zelda.Game
{
    public class Game : ITimerContext, IMenuContext
    {
        public Hero Hero { get; }
        public bool IsPaused { get; private set; }
        public bool IsDialogEnabled => _dialogBox.IsEnabled;
        public bool IsSuspended => CurrentMap == null || IsPaused;
        public Map CurrentMap { get; private set; }

        internal Equipment Equipment => SaveGame.Equipment;
        internal Savegame SaveGame { get; }
        internal GameCommands Commands { get; }
        internal CommandsEffects CommandsEffects { get; } = new CommandsEffects();

        internal bool CanPause => !IsSuspended && Equipment.GetLife() > 0; // 게임 오버 처리가 시작되려고 할 때에는 일시 정지를 허용하면 안 됩니다
        internal bool CanUnpause => IsPaused;
        internal bool HasCurrentMap => CurrentMap != null;

        readonly DialogBoxSystem _dialogBox;
        bool _started;
        Map _nextMap;

        public Game(Savegame saveGame)
        {
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
                if (Core.Mod.ResourceExists(ResourceType.Map, startingMapId))
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
                var maps = Core.Mod.GetResources(ResourceType.Map);
                if (maps.Count <= 0)
                    Debug.Die("This mod has no map");

                startingMapId = maps.First().Key;
                startingDestinationName = String.Empty;
            }

            SetCurrentMap(startingMapId, startingDestinationName);
        }

        internal void Start()
        {
            if (_started)
                return;

            _started = true;
            OnStarted();
        }

        protected virtual void OnStarted() { }

        internal void Stop()
        {
            if (!_started)
                return;

            if (CurrentMap != null)
            {
                if (Hero.IsOnMap)
                    Hero.NotifyBeingRemoved();
            }

            OnFinished();
            Timer.RemoveTimers(this);
            Menu.RemoveMenus(this);
            SaveGame.NotifyGameFinished();
            SaveGame.Game = null;

            _started = false;
        }

        protected virtual void OnFinished()
        {
        }

        internal void Restart()
        {
            throw new NotImplementedException();
        }

        internal bool NotifyInput(InputEvent inputEvent)
        {
            if (CurrentMap != null && CurrentMap.IsLoaded)
                Commands.NotifyInput(inputEvent);
            return true;
        }

        internal void Update()
        {
            UpdateTransitions();

            if (!_started)
                return;

            CurrentMap.Update();

            Equipment.Update();
            UpdateCommandsEffects();
        }

        internal void Draw(Surface dstSurface)
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

            ScriptContext.GameOnDraw(this, dstSurface);
        }

        internal bool NotifyDialogStarted(Dialog dialog, object info)
        {
            return OnDialogStarted(dialog, info);
        }

        protected virtual bool OnDialogStarted(Dialog dialog, object info) { return false; }

        internal void NotifyDialogFinished(Dialog dialog, Action<object> callback, object status)
        {
            OnDialogFinished(dialog);
            callback?.Invoke(status);
        }

        protected virtual void OnDialogFinished(Dialog dialog) { }

        internal void SetPaused(bool paused)
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
            if (!Core.Mod.DialogExists(dialogId))
                throw new Exception("No such dialog: '{0}'".F(dialogId));

            _dialogBox.Open(dialogId, info, callback);
        }

        public void StopDialog(object status)
        {
            if (!IsDialogEnabled)
                throw new InvalidOperationException("Cannot stop dialog: no dialog is active.");

            _dialogBox.Close(status);
        }

        internal void NotifyCommandPressed(GameCommand command)
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

        internal void SetCurrentMap(string mapId, string destinationName)
        {
            // 다음 맵을 준비합니다
            if (CurrentMap == null || mapId != CurrentMap.Id)
            {
                _nextMap = CreateMap(mapId);
                _nextMap.Load(this);
            }
            else
            {
                // 동일한 맵입니다
                _nextMap = CurrentMap;
            }

            _nextMap.DestinationName = destinationName;
        }

        Map CreateMap(string mapId)
        {
            var type = Core.Mod.ObjectCreator.GetTypeById<Map>(mapId);
            if (type == null)
                return new Map(mapId);

            var args = new Dictionary<string, object>() { { "id", mapId } };
            return Core.Mod.ObjectCreator.CreateObject<Map>(type.Name, args);
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
