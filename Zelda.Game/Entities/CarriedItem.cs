using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    public class CarriedObject : Entity
    {
        public enum Behavior
        {
            Throw,
            Destroy,
            Keep
        }

        public override EntityType Type => EntityType.CarriedItem;

        internal bool IsBeingLifted { get; private set; } = true;
        internal bool IsBeingThrown { get; private set; }
        internal bool IsBroken => _isBreaking && (_mainSprite.IsAnimationFinished || CanExplode);
        internal override bool CanBeObstacle => false;
        internal bool CanExplode => _explosionDate != 0;
        internal bool WillExplodeSoon => CanExplode && Core.Now >= _explosionDate - 1500;
        internal override bool IsDeepWaterObstacle => false;
        internal override bool IsHoleObstacle => false;
        internal override bool IsLavaObstacle => false;
        internal override bool IsPrickleObstacle => false;
        internal override bool IsLadderObstacle => false;

        static readonly string[] LiftingTrajectories =
        {
            "0 0  0 0  -3 -3  -5 -3  -5 -2",
            "0 0  0 0  0 -1  0 -1  0 0",
            "0 0  0 0  3 -3  5 -3  5 -2",
            "0 0  0 0  0 -10  0 -12  0 0",
        };

        readonly Hero _hero;
        readonly string _destructionSoundId;
        bool _isBreaking;
        int _yIncrement;
        int _nextDownDate;
        int _itemHeight;
        Sprite _mainSprite;
        Sprite _shadowSprite;
        Direction4 _throwingDirection = Direction4.Right;
        int _explosionDate;

        internal CarriedObject(Hero hero, Entity originalEntity, string animationSetId, string destructionSoundId, int damageOnEnemies, int explosionDate)
            : base("", Direction4.Right, hero.Layer, new Point(0, 0), new Size(0, 0))
        {
            _hero = hero;
            _destructionSoundId = destructionSoundId;
            _explosionDate = explosionDate;

            var direction = hero.AnimationDirection;
            if ((int)direction % 2 == 0)
                XY = new Point(originalEntity.X, hero.Y);
            else
                XY = new Point(hero.X, originalEntity.Y);
            Origin = originalEntity.Origin;
            Size = originalEntity.Size;
            IsDrawnInYOrder = true;

            var movement = new PixelMovement(LiftingTrajectories[(int)direction], 100, false, true);
            _mainSprite = CreateSprite(animationSetId, "main");
            _mainSprite.EnablePixelCollisions();
            _mainSprite.SetCurrentAnimation("stopped");
            DefaultSpriteName = "main";
            SetMovement(movement);

            _shadowSprite = CreateSprite("entities/shadow", "shadow");
            _shadowSprite.SetCurrentAnimation("big");
            _shadowSprite.StopAnimation();
        }

        public void ThrowItem(Direction4 direction)
        {
            _throwingDirection = direction;
            IsBeingLifted = false;
            IsBeingThrown = true;

            Core.Audio?.Play("throw");

            _mainSprite.SetCurrentAnimation("stopped");
            _shadowSprite.StartAnimation();

            Y = _hero.Y;
            var movement = new StraightMovement(false, false);
            movement.SetSpeed(200);
            movement.SetAngle(Geometry.DegreesToRadians((int)direction * 90));
            ClearMovement();
            SetMovement(movement);

            _yIncrement = -2;
            _nextDownDate = Core.Now + 40;
            _itemHeight = 18;
        }

        void BreakItemOnGround()
        {
            Movement.Stop();

            switch (GroundBelow)
            {
                case Ground.Empty:
                    if (Layer == Layer.Low)
                        BreakItem();
                    else
                    {
                        Entities.SetEntityLayer(this, (Layer)((int)Layer - 1));
                        BreakItemOnGround();
                    }
                    break;

                case Ground.Hole:
                    Core.Audio?.Play("jump");
                    RemoveFromMap();
                    break;

                case Ground.DeepWater:
                case Ground.Lava:
                    Core.Audio?.Play("walk_on_water");
                    RemoveFromMap();
                    break;

                default:
                    BreakItem();
                    break;
            }

            IsBeingLifted = false;
            _isBreaking = true;
        }

        void BreakItem()
        {
            if (IsBeingThrown && _throwingDirection != Direction4.Down)
            {
                // 실제 그려지는 위치에서 파괴합니다
                Y = Y - _itemHeight;
            }

            Movement.Stop();
            _shadowSprite.StopAnimation();

            if (!CanExplode)
            {
                if (_destructionSoundId != null)
                    Core.Audio?.Play(_destructionSoundId);

                if (_mainSprite.HasAnimation("destroy"))
                    _mainSprite.SetCurrentAnimation("destroy");
                else
                    RemoveFromMap();
            }
            else
            {
                Console.WriteLine("Create explosion entity here");
                Core.Audio?.Play("explosion");
                if (IsBeingThrown)
                    RemoveFromMap();
            }

            IsBeingThrown = false;
            _isBreaking = true;
        }

        internal override void Update()
        {
            base.Update();

            if (IsSuspended)
                return;

            if (IsBeingLifted && Movement.IsFinished)
            {
                IsBeingLifted = false;

                ClearMovement();
                SetMovement(new FollowMovement(_hero, 0, -18, true));
            }
            else if (CanExplode && !_isBreaking)
            {
                if (Core.Now >= _explosionDate)
                    BreakItem();
                else if (WillExplodeSoon)
                {
                    var animation = _mainSprite.CurrentAnimation;
                    if (animation == "stopped")
                        _mainSprite.SetCurrentAnimation("stopped_explosion_soon");
                    else if (animation == "walking")
                        _mainSprite.SetCurrentAnimation("walking_explosion_soon");
                }
            }

            if (IsBroken)
                RemoveFromMap();
            else if (IsBeingThrown)
            {
                if (Movement.IsStopped || _yIncrement >= 7)
                    BreakItemOnGround();
                else
                {
                    while (Core.Now >= _nextDownDate)
                    {
                        _nextDownDate += 40;
                        _itemHeight -= _yIncrement;
                        ++_yIncrement;
                    }
                }
            }
        }

        internal void SetAnimationStopped()
        {
            if (!IsBeingLifted && !IsBeingThrown)
            {
                var animation = WillExplodeSoon ? "stopped_explosion_soon" : "stopped";
                _mainSprite.SetCurrentAnimation(animation);
            }
        }

        internal void SetAnimationWalking()
        {
            if (!IsBeingLifted && !IsBeingThrown)
            {
                var animation = WillExplodeSoon ? "walking_explosion_soon" : "walking";
                _mainSprite.SetCurrentAnimation(animation);
            }
        }

        internal override void DrawOnMap()
        {
            if (!IsBeingThrown)
                base.DrawOnMap();
            else
            {
                Map.DrawSprite(_shadowSprite, XY);
                Map.DrawSprite(_mainSprite, X, Y - _itemHeight);
            }
        }

        internal override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (IsBeingThrown)
                _shadowSprite.SetSuspended(suspended);

            if (!suspended && WhenSuspended != 0)
            {
                int diff = Core.Now - WhenSuspended;
                if (IsBeingThrown)
                    _nextDownDate += diff;
                if (CanExplode)
                    _explosionDate += diff;
            }
        }

        internal override bool IsNpcObstacle(Npc npc) => npc.IsSolid;
    }
}
