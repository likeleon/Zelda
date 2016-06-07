using System;
using System.ComponentModel;
using Zelda.Game.LowLevel;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    class Block : Detector
    {
        public bool IsPushable { get; set; }
        public bool IsPullable { get; set; }
        public override EntityType Type => EntityType.Block;

        internal override bool IsHoleObstacle => false;

        public int MaximumMoves
        {
            get { return _maximumMoves; }
            set
            {
                if (value < 0 || value > 2)
                    throw new Exception("MaximumMoves must be between 0 and 2");

                _initialMaximumMoves = value;
                _maximumMoves = value;
            }
        }

        static int _movingDelay = 500;

        int _maximumMoves;
        int _initialMaximumMoves;
        int _whenCanMove = Core.Now;
        Point _lastPosition;
        Point _initialPosition;
        bool _soundPlayed;

        public Block(BlockData data)
            : base(data.Name, data.Layer, data.XY, new Size(16, 16))
        {
            if (data.MaximumMoves < 0 || data.MaximumMoves > 2)
                throw new Exception("Invalid MaximumMoves: {0}".F(data.MaximumMoves));

            _maximumMoves = data.MaximumMoves;
            _initialMaximumMoves = data.MaximumMoves;
            _lastPosition = data.XY;
            _initialPosition = data.XY;
            IsPushable = data.Pushable;
            IsPullable = data.Pullable;


            SetCollisionModes(CollisionMode.Facing);
            Origin = new Point(8, 13);
            Direction = data.Direction;
            var sprite = CreateSprite(data.Sprite);
            IsDrawnInYOrder = sprite.Size.Height > 16;
        }

        internal override bool IsObstacleFor(Entity other) => other.IsBlockObstacle(this);
        internal override bool IsDestructibleObstacle(Destructible destructible) => true;
        internal override bool IsHeroObstacle(Hero hero) => Movement == null;

        internal override void NotifyCreated()
        {
            base.NotifyCreated();

            CheckCollisionWithDetectors();
        }

        internal override void NotifyCollision(Entity entityOverlapping, CollisionMode collisionMode)
        {
            entityOverlapping.NotifyCollisionWithBlock(this);
        }

        internal override bool NotifyActionCommandPressed()
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.Grab)
            {
                Hero.StartGrabbing();
                return true;
            }
            return false;
        }

        internal override bool StartMovementByHero()
        {
            bool pulling = Hero.IsGrabbingOrPulling;
            var allowedDirection = Direction;
            var heroDirection = Hero.AnimationDirection;
            if (pulling)
                heroDirection = (Direction4)(((int)heroDirection + 2) % 4);

            if (Movement != null ||                 // 이미 이동 중
                _maximumMoves == 0 ||               // 더 이상 움직일 수 없음
                Core.Now < _whenCanMove ||  // 당분간 움직일 수 없음
                (pulling && !IsPullable) ||
                (!pulling && !IsPushable) ||
                (allowedDirection != Direction4.None && heroDirection != allowedDirection))
            {
                return false;
            }

            int dx = X - Hero.X;
            int dy = Y - Hero.Y;

            SetMovement(new FollowMovement(Hero, dx, dy, false));
            _soundPlayed = false;
            
            return true;
        }

        internal override void StopMovementByHero()
        {
            ClearMovement();
            _whenCanMove = Core.Now + _movingDelay;

            if (XY != _lastPosition)
            {
                _lastPosition = XY;

                if (_maximumMoves == 1)
                    _maximumMoves = 0;
            }
        }

        internal override void NotifyMovingBy(Entity entity)
        {
        }

        internal override void NotifyMovedBy(Entity entity)
        {
        }

        internal override void NotifyPositionChanged()
        {
            if (Movement != null && !_soundPlayed)
            {
                Core.Audio?.Play("hero_pushes");
                _soundPlayed = true;
            }

            CheckCollisionWithDetectors();
        }

        public void Reset()
        {
            if (Movement != null)
            {
                // 주인공에 의해 당기거나 멀고 있는 상태
                ClearMovement();
                _whenCanMove = Core.Now + _movingDelay;
            }

            XY = _initialPosition;
            _lastPosition = _initialPosition;
            _maximumMoves = _initialMaximumMoves;
        }
    }

    public class BlockData : EntityData
    {
        public override EntityType Type => EntityType.Block;

        [DefaultValue(Direction4.None)]
        public Direction4 Direction { get; set; } = Direction4.None;

        public string Sprite { get; set; }
        public bool Pushable { get; set; }
        public bool Pullable {  get; set; }
        public int MaximumMoves { get; set; }

        internal override void CreateEntity(Map map)
        {
            map.Entities.AddEntity(new Block(this));
        }
    }
}
