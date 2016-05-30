using System;
using System.Collections.Generic;
using Zelda.Game.Heroes;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Hero : Entity
    {
        public override EntityType Type => EntityType.Hero;
        public bool IsInvincible { get; private set; }
        public Direction4 AnimationDirection => HeroSprites.AnimationDirection;
        public int NormalWalkingSpeed { get; private set; }

        internal HeroSprites HeroSprites { get; }
        internal State State { get; private set; }
        internal string StateName => State.Name;
        internal bool IsUsingItem => State.IsUsingItem;
        internal EquipmentItemUsage ItemBeingUsed => State.ItemBeingUsed;
        internal bool IsBrandishingTreasure => State.IsBrandishingTreasure;
        internal bool IsGrabbingOrPulling => State.IsGrabbingOrPulling;
        internal bool IsFree => State.IsFree;
        internal int WalkingSpeed { get; private set; }
        internal Direction8 WantedMovementDirection8 => State.WantedMovementDirection8;
        internal override bool IsShallowWaterObstacle => State.IsShallowWaterObstacle;
        internal override bool IsDeepWaterObstacle => State.IsDeepWaterObstacle;
        internal override bool IsHoleObstacle => State.IsHoleObstacle;
        internal override bool IsLavaObstacle => State.IsLadderObstacle;
        internal override bool IsPrickleObstacle => State.IsPrickleObstacle;
        internal override bool IsLadderObstacle => State.IsLadderObstacle;

        readonly List<State> _oldStates = new List<State>();
        int _endInvincibleDate;

        internal Hero(Equipment equipment)
            : base("hero", 0, Layer.Low, new Point(), new Size(16, 16))
        {
            NormalWalkingSpeed = 88;
            WalkingSpeed = NormalWalkingSpeed;
            Origin = new Point(8, 13);
            IsDrawnInYOrder = true;
            HeroSprites = new HeroSprites(this, equipment);

            SetState(new FreeState(this));
        }

        internal void RebuildEquipment()
        {
            HeroSprites.RebuildEquipment();
        }

        public void SetInvincible(bool invincible, int duration)
        {
            IsInvincible = invincible;
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

        internal virtual bool CanBeHurt(Entity attacker)
        {
            return !IsInvincible && State.CanBeHurt(attacker);
        }

        internal void Hurt(Entity source, Sprite sourceSprite, int damage)
        {
            var sourceXY = source.XY;
            if (sourceSprite != null)
                sourceXY += sourceSprite.XY;

            SetState(new HurtState(this, sourceXY, damage));
        }


        internal void SetState(State newState)
        {
            // 이전 상태를 정지시킵니다
            State oldState = State;
            if (oldState != null)
            {
                oldState.Stop(newState);    // 여기서 다시 상태를 바꾸면 안 됩니다

                // 유효성 검사
                if (oldState != State)
                {
                    Debug.Error("Hero state '{0}' did not stop properly to let state '{1}' go, " +
                        "it started state '{2}' instead. " +
                        "State '{3}' will be forced.".F(oldState.Name, newState.Name, State.Name, newState.Name));

                    // 원래 시작하려고 했던 상태를 시작합니다.
                    // oldState는 이미 oldStates 리스트에 포함된 상태입니다
                    SetState(newState);
                    return;
                }
            }

            // 이전 상태가 이 함수의 호출자일 수 있기 때문에, 이전 상태를 바로 파괴하지 않고 파괴를 지연시키도록 합니다.
            _oldStates.Add(State);
            
            State = newState;
            State.Start(oldState); // 여기서 상태를 다시 바꿀 수도 있습니다

            if (State == newState) // 다시 상태가 바뀐게 아니라면
                CheckPosition();
        }

        void UpdateState()
        {
            State.Update();
            _oldStates.Clear();
        }

        internal void StartFree()
        {
            SetState(new FreeState(this));
        }

        internal void StartFreeCarryingLoadingOrRunning()
        {
            if (State.IsCarryingItem)
                SetState(new CarryingState(this, State.CarriedItem));
            else
                SetState(new FreeState(this));
        }

        internal void StartFreezed()
        {
            SetState(new FreezedState(this));
        }

        internal void StartGrabbing()
        {
            SetState(new GrabbingState(this));
        }

        internal void StartLifting(CarriedObject itemToLift)
        {
            SetState(new LiftingState(this, itemToLift));
        }

        internal void StartTreasure(Treasure treasure, Action callback)
        {
            SetState(new TreasureState(this, treasure, callback));
        }

        internal bool CanStartItem(EquipmentItem item)
        {
            if (!item.IsAssignable)
                return false;

            if (item.Variant == 0)
                return false;

            return State.CanStartItem(item);
        }

        internal void StartItem(EquipmentItem item)
        {
            Debug.CheckAssertion(CanStartItem(item), "The hero cannot start using item '{0}' now".F(item.Name));
            SetState(new UsingItemState(this, item));
        }

        internal void StartStateFromGround()
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

        public void SetNormalWalkingSpeed(int normalWalkingSpeed)
        {
            bool wasNormal = (WalkingSpeed == NormalWalkingSpeed);
            NormalWalkingSpeed = normalWalkingSpeed;
            if (wasNormal)
                SetWalkingSpeed(normalWalkingSpeed);
        }

        internal void SetWalkingSpeed(int walkingSpeed)
        {
            if (walkingSpeed != WalkingSpeed)
                WalkingSpeed = walkingSpeed;
        }

        // 장애물을 고려한 주인공의 실제 이동 방향을 반환합니다
        internal Direction8 GetRealMovementDirection8()
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

        internal override void NotifyMovementChanged()
        {
            Direction8 wantedDirection8 = WantedMovementDirection8;
            if (wantedDirection8 != Direction8.None)
            {
                Direction4 oldAnimationDirection = HeroSprites.AnimationDirection;
                Direction4 animationDirection = HeroSprites.GetAnimationDirection(wantedDirection8, GetRealMovementDirection8());

                if (animationDirection != oldAnimationDirection &&
                    animationDirection != Direction4.None)
                {
                    HeroSprites.SetAnimationDirection(animationDirection);
                }
            }

            State.NotifyMovementChanged();
            CheckPosition();
        }

        internal override void NotifyMovementFinished()
        {
            State.NotifyMovementFinished();
        }

        internal override void NotifyPositionChanged()
        {
            CheckPosition();
            State.NotifyPositionChanged();

        }

        internal override void NotifyLayerChanged()
        {
            State.NotifyLayerChanged();
        }

        internal void UpdateMovement()
        {
            Movement?.Update();

            // TODO: ClearOldMovements() 누락
        }

        internal override void Update()
        {
            UpdateInvincibility();
            UpdateMovement();
            HeroSprites.Update();

            // 상태는 이동과 스프라이트에 영향을 받기 때문에 이 시점에 업데이트를 수행합니다
            UpdateState();

            if (!IsSuspended)
                CheckCollisionWithDetectors();
        }

        internal override void DrawOnMap()
        {
            if (State.IsHeroVisible)
                State.DrawOnMap();
        }

        internal override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended)
            {
                int diff = Core.Now - WhenSuspended;

                if (_endInvincibleDate != 0)
                    _endInvincibleDate += diff;
            }

            HeroSprites.SetSuspended(suspended);
            State.SetSuspended(suspended);
        }

        internal void NotifyCommandPressed(GameCommand command)
        {
            State.NotifyCommandPressed(command);
        }

        internal void SetMap(Map map, Direction4 initialDirection)
        {
            if (initialDirection != Direction4.None)
                HeroSprites.SetAnimationDirection(initialDirection);

            State.SetMap(map);
            
            base.SetMap(map);
        }

        internal void PlaceOnDestination(Map map, Rectangle previousMapLocation)
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

        internal override void NotifyMapStarted()
        {
            base.NotifyMapStarted();
            HeroSprites.NotifyMapStarted();

            // 이 시점에 맵을 결정할 수 있게 됩니다. 상태에게 알려줍니다.
            State.SetMap(Map);
        }

        internal override void NotifyTilesetChanged()
        {
            base.NotifyTilesetChanged();
            HeroSprites.NotifyTilesetChanged();
        }

        internal override Point GetFacingPoint()
        {
            return GetTouchingPoint(AnimationDirection);
        }

        internal override void NotifyFacingEntityChanged(Detector facingEntity)
        {
            if (facingEntity == null &&
                CommandsEffects.IsActionCommandActingOnFacingEntity)
            {
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
        }

        internal bool IsFacingObstacle()
        {
            Rectangle collisionBox = BoundingBox;
            switch (HeroSprites.AnimationDirection)
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

        internal bool IsFacingDirection4(Direction4 direction4)
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

        internal override bool IsObstacleFor(Entity other) => other.IsHeroObstacle(this);
        internal override bool IsBlockObstacle(Block block) => block.IsHeroObstacle(this);
        
        internal void CheckPosition()
        {
            if (!IsOnMap)
                return;

            if (State.AreCollisionsIgnored)
                return;

            FacingEntity = null;
            CheckCollisionWithDetectors();
        }

        internal override void NotifyCollisionWithDestructible(Destructible destructible, CollisionMode collisionMode)
        {
            destructible.NotifyCollisionWithHero(this, collisionMode);
        }

        internal override void NotifyCollisionWithChest(Chest chest)
        {
 	         if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None &&
                 IsFree &&
                 IsFacingDirection4(Direction4.Up) &&
                 !chest.IsOpen)
             {
                 CommandsEffects.ActionCommandEffect = ActionCommandEffect.Open;
             }
        }

        internal override void NotifyCollisionWithBlock(Block block)
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None && IsFree)
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.Grab;
        }

        internal override void NotifyCollisionWithBomb(Bomb bomb, CollisionMode collisionMode)
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None &&
                FacingEntity == bomb &&
                IsFree)
            {
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.Lift;
            }
        }

        internal override void NotifyCollisionWithExplosion(Explosion explosion, Sprite spriteOverlapping)
        {
            string spriteId = spriteOverlapping.AnimationSetId;
            if (!State.CanAvoidExplosion &&
                spriteId == HeroSprites.TunicSpriteId &&
                CanBeHurt(explosion))
            {
                Hurt(explosion, null, 2);
            }
        }
    }
}
