﻿
using Zelda.Game.Engine;
namespace Zelda.Game.Entities
{
    class Block : Detector
    {
        public Block(
            string name,
            Layer layer,
            Point xy,
            Direction4 direction,
            string spriteName,
            bool canBePushed,
            bool canBePulled,
            int maximumMoves)
            : base(CollisionMode.Facing, name, layer, xy, new Size(16, 16))
        {
            _maximumMoves = maximumMoves;
            _initialMaximumMoves = maximumMoves;
            _whenCanMove = EngineSystem.Now;
            _lastPosition = xy;
            _initialPosition = xy;
            IsPushable = canBePushed;
            IsPullable = canBePulled;

            Debug.CheckAssertion(maximumMoves >= 0 && maximumMoves <= 2,
                "MaximumMoves must be between 0 and 2");
            Origin = new Point(8, 13);
            Direction = direction;
            CreateSprite(spriteName);
            SetDrawnInYOrder(Sprite.Size.Height > 16);
        }

        public bool IsPushable { get; set; }
        public bool IsPullable { get; set; }

        int _maximumMoves;
        public int MaximumMoves
        {
            get { return _maximumMoves; }
            set
            {
                Debug.CheckAssertion(value >= 0 && value <= 2,
                    "MaximumMoves must be between 0 and 2");
                _initialMaximumMoves = value;
                _maximumMoves = value;
            }
        }

        int _initialMaximumMoves;
        uint _whenCanMove;
        Point _lastPosition;
        Point _initialPosition;
        static uint _movingDelay = 500;

        public override EntityType Type
        {
            get { return EntityType.Block; }
        }

        public override bool IsObstacleFor(MapEntity other)
        {
            return other.IsBlockObstacle(this);
        }

        public override bool IsHoleObstacle
        {
            get { return false; }
        }

        public override bool IsDestructibleObstacle(Destructible destructible)
        {
            return true;
        }

        public override bool IsHeroObstacle(Hero hero)
        {
            return Movement == null;
        }

        public override void NotifyCreated()
        {
            base.NotifyCreated();

            CheckCollisionWithDetectors();
        }

        public override void NotifyCollision(MapEntity entityOverlapping, CollisionMode collisionMode)
        {
            entityOverlapping.NotifyCollisionWithBlock(this);
        }

        public override bool NotifyActionCommandPressed()
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.Grab)
            {
                Hero.StartGrabbing();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            if (Movement != null)
            {
                // 주인공에 의해 당기거나 멀고 있는 상태
                ClearMovement();
                _whenCanMove = EngineSystem.Now + _movingDelay;
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
    }

    public class BlockXmlData : EntityXmlData
    {
        public Direction4? Direction { get; set; }
        public string Sprite { get; set; }
        public bool? Pushable { get; set; }
        public bool? Pullable { get; set; }
        public int? MaximumMoves { get; set; }
    }
}
