﻿using System;
using System.Collections.Generic;
using Zelda.Game.Engine;
using Zelda.Game.Heroes;

namespace Zelda.Game.Entities
{
    class Hero : MapEntity
    {
        public Hero(Equipment equipment)
            : base("hero", 0, Layer.Low, new Point(), new Size(16, 16))
        {
            Origin = new Point(8, 13);

            // 스프라이트
            SetDrawnInYOrder(true);
            _sprites = new HeroSprites(this, equipment);

            // 상태
            SetState(new FreeState(this));
        }

        #region 특성
        public override EntityType Type
        {
            get { return EntityType.Hero; }
        }
        #endregion

        #region 스프라이트
        readonly HeroSprites _sprites;
        public HeroSprites HeroSprites
        {
            get { return _sprites; }
        }

        public void RebuildEquipment()
        {
            _sprites.RebuildEquipment();
        }
        #endregion

        #region 상태
        State _state;
        public string StateName
        {
            get { return _state.Name; }
        }

        readonly List<State> _oldStates = new List<State>();

        void SetState(State newState)
        {
            // 이전 상태를 정지시킵니다
            State oldState = _state;
            if (oldState != null)
            {
                oldState.Stop(newState);    // 여기서 다시 상태를 바꾸면 안 됩니다

                // 유효성 검사
                if (oldState != _state)
                {
                    Debug.Error("Hero state '{0}' did not stop properly to let state '{1}' go, " +
                        "it started state '{2}' instead. " +
                        "State '{3}' will be forced.".F(oldState.Name, newState.Name, _state.Name, newState.Name));

                    // 원래 시작하려고 했던 상태를 시작합니다.
                    // oldState는 이미 oldStates 리스트에 포함된 상태입니다
                    SetState(newState);
                    return;
                }
            }

            // 이전 상태가 이 함수의 호출자일 수 있기 때문에, 이전 상태를 바로 파괴하지 않고 파괴를 지연시키도록 합니다.
            _oldStates.Add(_state);
            
            _state = newState;
            _state.Start(oldState); // 여기서 상태를 다시 바꿀 수도 있습니다
        }

        void UpdateState()
        {
            _state.Update();
            _oldStates.Clear();
        }
        #endregion

        #region 게임 루프
        public override void Update()
        {
            _sprites.Update();

            // 상태는 이동과 스프라이트에 영향을 받기 때문에 이 시점에 업데이트를 수행합니다
            UpdateState();
        }

        public override void DrawOnMap()
        {
            if (!IsDrawn())
                return;

            if (_state.IsHeroVisible)
                _state.DrawOnMap();
        }
        #endregion

        #region 맵 변경
        public void SetMap(Map map, int initialDirection)
        {
            if (initialDirection != -1)
                _sprites.SetAnimationDirection(initialDirection);

            _state.SetMap(map);
            
            base.SetMap(map);
        }

        public void PlaceOnDestination(Map map, Rectangle previousMapLocation)
        {
            string destinationName = map.DestinationName;

            if (destinationName == "_same")
            {
                throw new NotImplementedException("destinationName == '_same'");
            }
            else
            {
                int side = map.GetDestinationSide();

                if (side != -1)
                {
                    SetMap(map, -1);

                    switch (side)
                    {
                        case 0: // 우
                            X = map.Width;
                            Y = Y - map.Location.Y + previousMapLocation.Y;
                            break;

                        case 1: // 상
                            Y = 5;
                            X = X - map.Location.X + previousMapLocation.X;
                            break;

                        case 2: // 좌
                            X = 0;
                            Y = Y - map.Location.Y + previousMapLocation.Y;
                            break;

                        case 3: // 하
                            Y = map.Height + 5;
                            X = X - map.Location.X + previousMapLocation.X;
                            break;

                        default:
                            Debug.Die("Invalid destination side");
                            break;
                    }
                }
                else
                {
                    Destination destination = map.GetDestination();
                    if (destination == null)
                    {
                        // 개발 도중에 이 경우가 발생할 수 있습니다. 맵의 좌측 최상단에 위치시킵니다
                        Debug.Error("No valid destination on map '{0}'. Placing the hero at (0,0) instead.".F(map.Id));
                        SetMap(map, 3);
                        TopLeftXY = new Point(0, 0);
                    }
                    else
                    {
                        SetMap(map, destination.Direction);
                        XY = destination.XY;
                    }
                }
            }
        }
        #endregion
    }
}
