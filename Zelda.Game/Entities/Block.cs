using System;
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

        public Block(string name, Layer layer, Point xy, Direction4 direction, string spriteName, bool canBePushed, bool canBePulled, int maximumMoves)
            : base(name, layer, xy, new Size(16, 16))
        {
            _maximumMoves = maximumMoves;
            _initialMaximumMoves = maximumMoves;
            _lastPosition = xy;
            _initialPosition = xy;
            IsPushable = canBePushed;
            IsPullable = canBePulled;

            if (maximumMoves < 0 || maximumMoves > 2)
                throw new ArgumentOutOfRangeException(nameof(maximumMoves), "MaximumMoves must be between 0 and 2");

            SetCollisionModes(CollisionMode.Facing);
            Origin = new Point(8, 13);
            Direction = direction;
            var sprite = CreateSprite(spriteName);
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

    class BlockData : EntityData
    {
        public Direction4 Direction { get; set; }
        public string Sprite { get; set; }
        public bool Pushable { get; set; }
        public bool Pullable {  get; set; }
        public int MaximumMoves { get; set; }

        public BlockData(BlockXmlData xmlData)
            : base(EntityType.Block, xmlData)
        {
            Direction = xmlData.Direction.OptField("Direction", Direction4.None);
            Sprite = xmlData.Sprite.CheckField("entities/block");
            Pushable = xmlData.Pushable.CheckField("Pushable");
            Pullable = xmlData.Pullable.CheckField("Pullable");
            MaximumMoves = xmlData.MaximumMoves.CheckField("MaximumMoves");
        }

        protected override EntityXmlData ExportXmlData()
        {
            var data = new BlockXmlData();
            if (Direction != Direction4.None)
                data.Direction = Direction;
            data.Sprite = Sprite;
            data.Pushable = Pushable;
            data.Pullable = Pullable;
            data.MaximumMoves = MaximumMoves;
            return data;
        }
    }

    public class BlockXmlData : EntityXmlData
    {
        public Direction4? Direction { get; set; }
        public string Sprite { get; set; }
        public bool? Pushable { get; set; }
        public bool? Pullable { get; set; }
        public int? MaximumMoves { get; set; }

        public bool ShouldSerializeDirection() { return Direction != Direction4.None; }
    }
}
