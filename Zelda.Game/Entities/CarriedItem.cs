using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    class CarriedItem : MapEntity
    {
        public enum Behavior
        {
            Throw,
            Destroy,
            Keep
        }

        readonly static string[] _liftingTrajectories =
        {
            "0 0  0 0  -3 -3  -5 -3  -5 -2",
            "0 0  0 0  0 -1  0 -1  0 0",
            "0 0  0 0  3 -3  5 -3  5 -2",
            "0 0  0 0  0 -10  0 -12  0 0",
        };

        readonly string _destructionSoundId;

        public CarriedItem(
            Hero hero,
            MapEntity originalEntity,
            string animationSetId,
            string destructionSoundId,
            int damageOnEnemies,
            uint explosionDate)
            : base("", Direction4.Right, hero.Layer, new Point(0, 0), new Size(0, 0))
        {
            _hero = hero;
            _destructionSoundId = destructionSoundId;
            _explosionDate = explosionDate;
            IsBeingLifted = true;

            Direction4 direction = hero.AnimationDirection;
            if ((int)direction % 2 == 0)
                XY = new Point(originalEntity.X, hero.Y);
            else
                XY = new Point(hero.X, originalEntity.Y);
            Origin = originalEntity.Origin;
            Size = originalEntity.Size;
            SetDrawnInYOrder(true);

            PixelMovement movement = new PixelMovement(_liftingTrajectories[(int)direction], 100, false, true);
            CreateSprite(animationSetId);
            Sprite.SetCurrentAnimation("stopped");
            SetMovement(movement);

            _shadowSprite = new Sprite("entities/shadow");
            _shadowSprite.SetCurrentAnimation("big");
        }

        public override EntityType Type
        {
            get { return EntityType.CarriedItem; }
        }

        public override bool CanBeObstacle
        {
            get { return false; }
        }

        #region 게임 데이터
        readonly Hero _hero;
        #endregion

        #region 둘기
        public bool IsBeingLifted { get; private set; }
        #endregion

        #region 던지기
        public bool IsBeingThrown { get; private set; }

        Direction4 _throwingDirection = Direction4.Right;
        int _yIncrement;
        uint _nextDownDate;
        int _itemHeight;
        Sprite _shadowSprite;

        public void ThrowItem(Direction4 direction)
        {
            _throwingDirection = direction;
            IsBeingLifted = false;
            IsBeingThrown = true;

            Sound.Play("throw");

            Sprite.SetCurrentAnimation("stopped");

            Y = _hero.Y;
            StraightMovement movement = new StraightMovement(false, false);
            movement.SetSpeed(200);
            movement.SetAngle(Geometry.DegreesToRadians((int)direction * 90));
            ClearMovement();
            SetMovement(movement);

            _yIncrement = -2;
            _nextDownDate = MainLoop.Now + 40;
            _itemHeight = 18;
        }
        #endregion

        #region 파괴
        bool _isBreaking;
        public bool IsBroken
        {
            get { return _isBreaking && (Sprite.IsAnimationFinished || CanExplode); }
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
                    Sound.Play("jump");
                    RemoveFromMap();
                    break;

                case Ground.DeepWater:
                case Ground.Lava:
                    Sound.Play("walk_on_water");
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

            if (!CanExplode)
            {
                if (!String.IsNullOrEmpty(_destructionSoundId))
                    Sound.Play(_destructionSoundId);

                if (Sprite.HasAnimation("destroy"))
                    Sprite.SetCurrentAnimation("destroy");
            }
            else
            {
                Console.WriteLine("Create explosion entity here");
                Sound.Play("explosion");
                if (IsBeingThrown)
                    RemoveFromMap();
            }

            IsBeingThrown = false;
            _isBreaking = true;
        }

        #endregion

        #region 폭파
        uint _explosionDate;

        public bool CanExplode
        {
            get { return _explosionDate != 0; }
        }

        public bool WillExplodeSoon
        {
            get { return CanExplode && MainLoop.Now >= _explosionDate - 1500; }
        }
        #endregion

        public override void Update()
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
                if (MainLoop.Now >= _explosionDate)
                    BreakItem();
                else if (WillExplodeSoon)
                {
                    string animation = Sprite.CurrentAnimation;
                    if (animation == "stopped")
                        Sprite.SetCurrentAnimation("stopped_explosion_soon");
                    else if (animation == "walking")
                        Sprite.SetCurrentAnimation("walking_explosion_soon");
                }
            }

            if (IsBeingThrown)
            {
                _shadowSprite.Update();

                if (IsBroken)
                    RemoveFromMap();
                else if (Movement.IsStopped || _yIncrement >= 7)
                    BreakItemOnGround();
                else
                {
                    uint now = MainLoop.Now;
                    while (now >= _nextDownDate)
                    {
                        _nextDownDate += 40;
                        _itemHeight -= _yIncrement;
                        ++_yIncrement;
                    }
                }
            }
        }

        public void SetAnimationStopped()
        {
            if (!IsBeingLifted && !IsBeingThrown)
                Sprite.SetCurrentAnimation("stopped");
        }

        public void SetAnimationWalking()
        {
            if (!IsBeingLifted && !IsBeingThrown)
                Sprite.SetCurrentAnimation("walking");
        }

        public override void DrawOnMap()
        {
            if (!IsDrawn())
                return;

            if (!IsBeingThrown)
                base.DrawOnMap();
            else
            {
                Map.DrawSprite(_shadowSprite, XY);
                Map.DrawSprite(Sprite, X, Y - _itemHeight);
            }
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (IsBeingThrown)
                _shadowSprite.SetSuspended(suspended);

            if (!suspended && WhenSuspended != 0)
            {
                uint diff = MainLoop.Now - WhenSuspended;
                if (IsBeingThrown)
                    _nextDownDate += diff;
                if (CanExplode)
                    _explosionDate += diff;
            }
        }

        #region 장애물 & 충돌
        public override bool IsDeepWaterObstacle
        {
            get { return false; }
        }

        public override bool IsHoleObstacle
        {
            get { return false; }
        }

        public override bool IsLavaObstacle
        {
            get { return false; }
        }

        public override bool IsPrickleObstacle
        {
            get { return false; }
        }

        public override bool IsLadderObstacle
        {
            get { return false; }
        }

        public override bool IsNpcObstacle(Npc npc)
        {
            return npc.IsSolid;
        }
        #endregion
    }
}
