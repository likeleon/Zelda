using Zelda.Game.LowLevel;
using Zelda.Game.Movements;
using Zelda.Game.Script;

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
            _whenCanMove = Core.Now;
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

            _scriptBlock = new ScriptBlock(this);
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
        int _whenCanMove;
        Point _lastPosition;
        Point _initialPosition;
        bool _soundPlayed;
        static int _movingDelay = 500;

        public override EntityType Type
        {
            get { return EntityType.Block; }
        }

        readonly ScriptBlock _scriptBlock;
        public override ScriptEntity ScriptEntity
        {
            get { return _scriptBlock; }
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

        public override bool StartMovementByHero()
        {
            bool pulling = Hero.IsGrabbingOrPulling;
            Direction4 allowedDirection = Direction;
            Direction4 heroDirection = Hero.AnimationDirection;
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

        public override void StopMovementByHero()
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

        public override void NotifyMovingBy(MapEntity entity)
        {
        }

        public override void NotifyMovedBy(MapEntity entity)
        {
        }

        public override void NotifyPositionChanged()
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
