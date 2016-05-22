using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;
using Zelda.Game.Heroes;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    class Hero : MapEntity
    {
        public Hero(Equipment equipment)
            : base("hero", 0, Layer.Low, new Point(), new Size(16, 16))
        {
            _normalWalkingSpeed = 88;
            _walkingSpeed = _normalWalkingSpeed;
            _scriptHero = new ScriptHero(this);

            Origin = new Point(8, 13);

            // 스프라이트
            IsDrawnInYOrder = true;
            _sprites = new HeroSprites(this, equipment);

            // 상태
            SetState(new FreeState(this));
        }

        #region 특성
        public override EntityType Type
        {
            get { return EntityType.Hero; }
        }

        readonly ScriptHero _scriptHero;
        public override ScriptEntity ScriptEntity
        {
            get { return _scriptHero; }
        }
        #endregion

        #region 스프라이트
        readonly HeroSprites _sprites;
        public HeroSprites HeroSprites
        {
            get { return _sprites; }
        }

        public Direction4 AnimationDirection
        {
            get { return _sprites.AnimationDirection; }
        }

        public void RebuildEquipment()
        {
            _sprites.RebuildEquipment();
        }
        #endregion

        #region 적들
        bool _invincible;
        public bool IsInvincible
        {
            get { return _invincible; }
        }

        int _endInvincibleDate;

        public void SetInvincible(bool invincible, int duration)
        {
            _invincible = invincible;
            _endInvincibleDate = 0;
            if (invincible)
                _endInvincibleDate = Core.Now + duration;
        }

        void UpdateInvincibility()
        {
            if (IsInvincible &&
                Core.Now >= _endInvincibleDate)
                SetInvincible(false, 0);
        }

        public virtual bool CanBeHurt(MapEntity attacker)
        {
            return !IsInvincible && _state.CanBeHurt(attacker);
        }

        public void Hurt(MapEntity source, Sprite sourceSprite, int damage)
        {
            Point sourceXY = source.XY;
            if (sourceSprite != null)
                sourceXY += sourceSprite.XY;

            SetState(new HurtState(this, sourceXY, damage));
        }
        #endregion

        #region 상태
        State _state;
        internal State State
        {
            get { return _state; }
        }

        public string StateName
        {
            get { return _state.Name; }
        }

        public bool IsUsingItem
        {
            get { return _state.IsUsingItem; }
        }

        public EquipmentItemUsage ItemBeingUsed
        {
            get { return _state.ItemBeingUsed; }
        }

        public bool IsBrandishingTreasure
        {
            get { return _state.IsBrandishingTreasure; }
        }

        public bool IsGrabbingOrPulling
        {
            get { return _state.IsGrabbingOrPulling; }
        }

        public bool IsFree
        {
            get { return _state.IsFree; }
        }

        readonly List<State> _oldStates = new List<State>();

        internal void SetState(State newState)
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

            if (_state == newState) // 다시 상태가 바뀐게 아니라면
                CheckPosition();
        }

        void UpdateState()
        {
            _state.Update();
            _oldStates.Clear();
        }

        public void StartFree()
        {
            SetState(new FreeState(this));
        }

        public void StartFreeCarryingLoadingOrRunning()
        {
            if (_state.IsCarryingItem)
                SetState(new CarryingState(this, _state.CarriedItem));
            else
                SetState(new FreeState(this));
        }

        public void StartFreezed()
        {
            SetState(new FreezedState(this));
        }

        public void StartGrabbing()
        {
            SetState(new GrabbingState(this));
        }

        public void StartLifting(CarriedItem itemToLift)
        {
            SetState(new LiftingState(this, itemToLift));
        }

        public void StartTreasure(Treasure treasure, Action callback)
        {
            SetState(new TreasureState(this, treasure, callback));
        }

        public bool CanStartItem(EquipmentItem item)
        {
            if (!item.IsAssignable)
                return false;

            if (item.Variant == 0)
                return false;

            return _state.CanStartItem(item);
        }

        public void StartItem(EquipmentItem item)
        {
            Debug.CheckAssertion(CanStartItem(item), "The hero cannot start using item '{0}' now".F(item.Name));
            SetState(new UsingItemState(this, item));
        }

        public void StartStateFromGround()
        {
            switch (GroundBelow)
            {
                case Ground.Traversable:
                case Ground.Empty:
                case Ground.Ladder:
                case Ground.Ice:
                    StartFreeCarryingLoadingOrRunning();
                    break;
            }
        }
        #endregion

        #region 이동
        int _normalWalkingSpeed;
        public int NormalWalkingSpeed
        {
            get { return _normalWalkingSpeed; }
        }

        public void SetNormalWalkingSpeed(int normalWalkingSpeed)
        {
            bool wasNormal = (_walkingSpeed == _normalWalkingSpeed);
            _normalWalkingSpeed = normalWalkingSpeed;
            if (wasNormal)
                SetWalkingSpeed(normalWalkingSpeed);
        }

        int _walkingSpeed;
        public int WalkingSpeed
        {
            get { return _walkingSpeed; }
        }

        public void SetWalkingSpeed(int walkingSpeed)
        {
            if (walkingSpeed != _walkingSpeed)
                _walkingSpeed = walkingSpeed;
        }

        public Direction8 WantedMovementDirection8
        {
            get { return _state.WantedMovementDirection8; }
        }

        // 장애물을 고려한 주인공의 실제 이동 방향을 반환합니다
        public Direction8 GetRealMovementDirection8()
        {
            Direction8 result = 0;

            Direction8 wantedDirection8 = WantedMovementDirection8;
            if (wantedDirection8 == Direction8.None)
                result = Direction8.None;
            else
            {
                Rectangle collisionBox = BoundingBox;

                Point xyMove = DirectionToXyMove(wantedDirection8);
                collisionBox.AddXY(xyMove);
                if (!Map.TestCollisionWithObstacles(Layer, collisionBox, this))
                    result = wantedDirection8;
                else
                {
                    // 원하는 방향으로 갈 수 없다면 가까운 두 방향 중 하나로 이동할 수 있는지 확인합니다
                    Direction8 alternativeDirection8 = (Direction8)(((int)wantedDirection8 + 1) % 8);
                    collisionBox = BoundingBox;
                    xyMove = DirectionToXyMove(alternativeDirection8);
                    collisionBox.AddXY(xyMove);
                    if (!Map.TestCollisionWithObstacles(Layer, collisionBox, this))
                        result = alternativeDirection8;
                    else
                    {
                        alternativeDirection8 = (Direction8)(((int)wantedDirection8 + 7) % 8);
                        collisionBox = BoundingBox;
                        xyMove = DirectionToXyMove(alternativeDirection8);
                        collisionBox.AddXY(xyMove);
                        if (!Map.TestCollisionWithObstacles(Layer, collisionBox, this))
                            result = alternativeDirection8;
                        else
                            result = wantedDirection8;  // 이동을 원하지만 이동할 수 없고 슬라이딩은 불가
                    }
                }
            }

            return result;
        }

        public override void NotifyMovementChanged()
        {
            Direction8 wantedDirection8 = WantedMovementDirection8;
            if (wantedDirection8 != Direction8.None)
            {
                Direction4 oldAnimationDirection = _sprites.AnimationDirection;
                Direction4 animationDirection = _sprites.GetAnimationDirection(wantedDirection8, GetRealMovementDirection8());

                if (animationDirection != oldAnimationDirection &&
                    animationDirection != Direction4.None)
                {
                    _sprites.SetAnimationDirection(animationDirection);
                }
            }

            _state.NotifyMovementChanged();
            CheckPosition();
        }

        public override void NotifyMovementFinished()
        {
            _state.NotifyMovementFinished();
        }

        public override void NotifyPositionChanged()
        {
            CheckPosition();
            _state.NotifyPositionChanged();

        }

        public override void NotifyLayerChanged()
        {
            _state.NotifyLayerChanged();
        }

        internal void UpdateMovement()
        {
            if (Movement != null)
                Movement.Update();

            // TODO: ClearOldMovements() 누락
        }
        #endregion

        #region 게임 루프
        public override void Update()
        {
            UpdateInvincibility();
            UpdateMovement();
            _sprites.Update();

            // 상태는 이동과 스프라이트에 영향을 받기 때문에 이 시점에 업데이트를 수행합니다
            UpdateState();

            if (!IsSuspended)
                CheckCollisionWithDetectors();
        }

        public override void DrawOnMap()
        {
            if (_state.IsHeroVisible)
                _state.DrawOnMap();
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended)
            {
                int diff = Core.Now - WhenSuspended;

                if (_endInvincibleDate != 0)
                    _endInvincibleDate += diff;
            }

            _sprites.SetSuspended(suspended);
            _state.SetSuspended(suspended);
        }

        public void NotifyCommandPressed(GameCommand command)
        {
            _state.NotifyCommandPressed(command);
        }
        #endregion

        #region 맵 변경
        public void SetMap(Map map, Direction4 initialDirection)
        {
            if (initialDirection != Direction4.None)
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
                    SetMap(map, Direction4.None);

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
                        SetMap(map, Direction4.Down);
                        TopLeftXY = new Point(0, 0);
                        map.Entities.SetEntityLayer(this, Layer.High);
                    }
                    else
                    {
                        SetMap(map, destination.Direction);
                        XY = destination.XY;
                        map.Entities.SetEntityLayer(this, destination.Layer);
                    }

                    CheckPosition();    // 예를 들면 수영 중인 상태로 시작하기 위해서 필요합니다
                }
            }
        }

        public override void NotifyMapStarted()
        {
            base.NotifyMapStarted();
            _sprites.NotifyMapStarted();

            // 이 시점에 맵을 결정할 수 있게 됩니다. 상태에게 알려줍니다.
            _state.SetMap(Map);
        }

        public override void NotifyTilesetChanged()
        {
            base.NotifyTilesetChanged();
            _sprites.NotifyTilesetChanged();
        }
        #endregion

        #region 위치
        public override Point GetFacingPoint()
        {
            return GetTouchingPoint(AnimationDirection);
        }

        public override void NotifyFacingEntityChanged(Detector facingEntity)
        {
            if (facingEntity == null &&
                CommandsEffects.IsActionCommandActingOnFacingEntity)
            {
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
        }

        public bool IsFacingObstacle()
        {
            Rectangle collisionBox = BoundingBox;
            switch (_sprites.AnimationDirection)
            {
                case Direction4.Right:
                    collisionBox.AddX(1);
                    break;

                case Direction4.Up:
                    collisionBox.AddY(-1);
                    break;

                case Direction4.Left:
                    collisionBox.AddX(-1);
                    break;

                case Direction4.Down:
                    collisionBox.AddY(1);
                    break;

                default:
                    Debug.Die("Invalid animation direction");
                    break;
            }

            return Map.TestCollisionWithObstacles(Layer, collisionBox, this);
        }

        public bool IsFacingDirection4(Direction4 direction4)
        {
            return AnimationDirection == direction4;
        }

        internal void TrySnapToFacingEntity()
        {
            Rectangle collisionBox = BoundingBox;
            if ((int)AnimationDirection % 2 == 0)
            {
                if (Math.Abs(collisionBox.Y - FacingEntity.TopLeftY) <= 5)
                    collisionBox.Y = FacingEntity.TopLeftY;
            }
            else
            {
                if (Math.Abs(collisionBox.X - FacingEntity.TopLeftX) <= 5)
                    collisionBox.X = FacingEntity.TopLeftX;
            }

            if (!Map.TestCollisionWithObstacles(Layer, collisionBox, this))
            {
                BoundingBox = collisionBox;
                NotifyPositionChanged();
            }
        }
        #endregion
        
        #region Obstacles
        public override bool IsObstacleFor(MapEntity other)
        {
            return other.IsHeroObstacle(this);
        }

        public override bool IsShallowWaterObstacle
        {
            get { return _state.IsShallowWaterObstacle; }
        }

        public override bool IsDeepWaterObstacle
        {
            get { return _state.IsDeepWaterObstacle; }
        }

        public override bool IsHoleObstacle
        {
            get { return _state.IsHoleObstacle; }
        }

        public override bool IsLavaObstacle
        {
            get { return _state.IsLavaObstacle; }
        }

        public override bool IsPrickleObstacle
        {
            get { return _state.IsPrickleObstacle; }
        }

        public override bool IsLadderObstacle
        {
            get { return _state.IsLadderObstacle; }
        }

        public override bool IsBlockObstacle(Block block)
        {
            return block.IsHeroObstacle(this);
        }
        #endregion
        
        #region 충돌
        public void CheckPosition()
        {
            if (!IsOnMap)
                return;

            if (_state.AreCollisionsIgnored)
                return;

            FacingEntity = null;
            CheckCollisionWithDetectors();
        }

        public override void NotifyCollisionWithDestructible(Destructible destructible, CollisionMode collisionMode)
        {
            destructible.NotifyCollisionWithHero(this, collisionMode);
        }

        public override void NotifyCollisionWithChest(Chest chest)
        {
 	         if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None &&
                 IsFree &&
                 IsFacingDirection4(Direction4.Up) &&
                 !chest.IsOpen)
             {
                 CommandsEffects.ActionCommandEffect = ActionCommandEffect.Open;
             }
        }

        public override void NotifyCollisionWithBlock(Block block)
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None && IsFree)
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.Grab;
        }

        public override void NotifyCollisionWithBomb(Bomb bomb, CollisionMode collisionMode)
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None &&
                FacingEntity == bomb &&
                IsFree)
            {
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.Lift;
            }
        }

        public override void NotifyCollisionWithExplosion(Explosion explosion, Sprite spriteOverlapping)
        {
            string spriteId = spriteOverlapping.AnimationSetId;
            if (!_state.CanAvoidExplosion &&
                spriteId == HeroSprites.TunicSpriteId &&
                CanBeHurt(explosion))
            {
                Hurt(explosion, null, 2);
            }
        }
        #endregion
    }
}
