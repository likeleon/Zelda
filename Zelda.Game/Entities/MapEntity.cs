using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    public abstract class MapEntity : DisposableObject
    {
        public Layer Layer { get; private set; }

        public Size Size
        {
            get { return _boundingBox.Size; }
            set
            {
                if (value.Width % 8 != 0 || value.Height % 8 != 0)
                    throw new Exception("Invalid entity size: width and height must be multiple of 8");
                _boundingBox.Size = value;
            }
        }

        public Point Origin
        {
            get { return _origin; }
            set
            {
                _boundingBox.XY += (_origin - value);
                _origin = value;
            }
        }

        public Point XY
        {
            get { return new Point(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public int X
        {
            get { return _boundingBox.X + _origin.X; }
            set { _boundingBox.X = value - _origin.X; }
        }

        public int Y
        {
            get { return _boundingBox.Y + _origin.Y; }
            set { _boundingBox.Y = value - _origin.Y; }
        }

        public Point CenterPoint { get { return BoundingBox.Center; } }

        public int OptimizationDistance
        {
            get { return _optimizationDistance; }
            set
            {
                _optimizationDistance = value;
                OptimizationDistance2 = value * value;
            }
        }

        public bool IsVisible { get; private set; } = true;
        public bool IsEnabled { get; private set; } = true;
        public virtual bool HasLayerIndependentCollisions => false;
        public virtual Ground ModifiedGround => Ground.Empty;
        public abstract EntityType Type { get; }

        internal Map Map { get; private set; }
        internal bool IsOnMap => Map != null;
        internal bool IsBeingRemoved { get; private set; }
        internal bool IsHero => Type == EntityType.Hero;
        internal bool IsDrawnInYOrder { get; set; }
        internal string Name { get; set; }
        internal Ground GroundBelow { get; private set; }
        internal Game Game => Map.Game;
        internal Direction4 Direction { get; set; }
        internal int Width => BoundingBox.Width;
        internal int Height => BoundingBox.Height;

        internal Rectangle BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        internal int TopLeftX
        {
            get { return _boundingBox.X; }
            set { _boundingBox.X = value; }
        }

        internal int TopLeftY
        {
            get { return _boundingBox.Y; }
            set { _boundingBox.Y = value; }
        }

        internal Point TopLeftXY
        {
            get { return new Point(TopLeftX, TopLeftY); }
            set { TopLeftX = value.X; TopLeftY = value.Y; }
        }

        internal Detector FacingEntity
        {
            get { return _facingEntity; }
            set
            {
                _facingEntity = value;
                NotifyFacingEntityChanged(value);
            }
        }

        internal int OptimizationDistance2 { get; private set; }

        internal bool IsAlignedToGrid => IsAlignedToGridX && IsAlignedToGridY;
        internal bool IsAlignedToGridX => TopLeftX % 8 == 0;
        internal bool IsAlignedToGridY => TopLeftY % 8 == 0;
        internal IEnumerable<Sprite> Sprites => _sprites;
        internal bool HasSprite => _sprites.Count > 0; 
        public Sprite Sprite => _sprites[0];
        internal bool IsSuspended { get; private set; }
        internal Movement Movement { get; private set; }

        internal virtual bool IsDrawnAtItsPosition => true;
        internal virtual bool CanBeDrawn => true;
        internal virtual bool CanBeObstacle => true;
        internal virtual bool IsDetector => false;
        internal virtual bool IsGroundModifier => false;

        internal virtual bool IsLowWallObstacle => true;
        internal virtual bool IsShallowWaterObstacle => IsDeepWaterObstacle;
        internal virtual bool IsDeepWaterObstacle => true;
        internal virtual bool IsHoleObstacle => true;
        internal virtual bool IsLavaObstacle => true;
        internal virtual bool IsPrickleObstacle => true;
        internal virtual bool IsLadderObstacle => true;

        protected MapEntities Entities => Map.Entities;

        internal CommandsEffects CommandsEffects => Game.CommandsEffects;
        internal Equipment Equipment => Game.Equipment;
        internal Hero Hero => Entities.Hero;
        internal Savegame Savegame => Game.SaveGame;
        internal int WhenSuspended { get; private set; }

        static readonly int DefaultOptimizationDistance = 400;
        static readonly Point[] DirectionsToXyMoves = new Point[]
        {
            new Point( 1,  0),
            new Point( 1, -1),
            new Point( 0, -1),
            new Point(-1, -1),
            new Point(-1,  0),
            new Point(-1,  1),
            new Point( 0,  1),
            new Point( 1,  1),
        };

        readonly List<Sprite> _sprites = new List<Sprite>();
        readonly List<Sprite> _oldSprites = new List<Sprite>();
        readonly List<Movement> _oldMovements = new List<Movement>();

        bool _initialized;
        Point _origin;
        Rectangle _boundingBox;
        int _optimizationDistance = DefaultOptimizationDistance;
        Detector _facingEntity;
        bool _waitingEnabled;

        protected MapEntity(string name, Direction4 direction, Layer layer, Point xy, Size size)
        {
            if (size.Width % 8 != 0 || size.Height % 8 != 0)
                throw new ArgumentOutOfRangeException("Invalid entity size: width and height must be multiple of 8");

            Name = name;
            Direction = direction;
            Layer = layer;
            _boundingBox = new Rectangle(xy, size);
            OptimizationDistance2 = DefaultOptimizationDistance * DefaultOptimizationDistance;
        }

        protected override void OnDispose(bool disposing)
        {
            ClearSprites();
            ClearOldSprites();
            ClearMovement();
            ClearOldMovements();
        }

        public void SetEnabled(bool enabled)
        {
            if (IsEnabled == enabled)
                return;

            if (enabled)
            {
                _waitingEnabled = true;
                return;
            }

            IsEnabled = false;
            _waitingEnabled = false;

            if (!IsSuspended)
            {
                if (Movement != null)
                    Movement.SetSuspended(true);

                foreach (var sprite in _sprites)
                    sprite.SetSuspended(true);

                if (IsOnMap)
                    Timer.SetEntityTimersSuspended(this, false);
            }
            // TODO: NotifyEnabled(false);
        }

        public void RemoveFromMap()
        {
            Map.Entities.RemoveEntity(this);
        }

        internal virtual void NotifyBeingRemoved()
        {
            IsBeingRemoved = true;
        }

        internal void SetMap(Map map)
        {
            Map = map;
            if (Game.CurrentMap == map)
                NotifyTilesetChanged();

            GroundBelow = Ground.Empty;

            if (!_initialized && Map.IsLoaded)
            {
                // 엔티티가 이미 실행중인 맵에 생성된 경우로, 초기화를 지금 바료 완료할 수 있습니다
                FinishInitialization();
            }
        }

        void FinishInitialization()
        {
            Debug.CheckAssertion(!_initialized, "Entity is already initialized");
            Debug.CheckAssertion(IsOnMap, "Missing map");
            Debug.CheckAssertion(Map.IsLoaded, "Map is not ready");

            _initialized = true;

            NotifyCreating();
            //ScriptContext.EntityOnCreated(this);
            NotifyCreated();
        }

        internal virtual void NotifyMapStarted()
        {
            if (!_initialized)
                FinishInitialization();
        }

        internal virtual void NotifyCreating()
        {
        }

        internal virtual void NotifyCreated()
        {
        }

        internal virtual void NotifyTilesetChanged()
        {
            foreach (Sprite sprite in _sprites)
                sprite.SetTileset(Map.Tileset);
        }

        internal void SetLayer(Layer layer)
        {
            Layer = layer;
            NotifyLayerChanged();
        }

        internal Point GetDisplayedXY()
        {
            return Movement?.GetDisplayedXY() ?? XY;
        }

        // 엔티티가 바라보는 위치 좌표를 반환합니다.
        // 스프라이트가 있다면 스프라이트의 방향에 기반한 위치입니다.
        // 스프라이트가 없거나 스프라이트가 4방향이 아니라면 이동을 고려한 위치입니다.
        // 이동도 없다면, 북쪽을 바라보는 것으로 가정합니다.
        internal virtual Point GetFacingPoint()
        {
            Direction4 direction4 = Direction4.Up; // 기본으로 북쪽
            if (HasSprite && Sprite.NumDirections == 4)
                direction4 = Sprite.CurrentDirection;
            else if (Movement != null)
                direction4 = Movement.GetDisplayedDirection4();

            return GetTouchingPoint(direction4);
        }

        internal Point GetTouchingPoint(Direction4 direction)
        {
            var touchingPoint = CenterPoint;
            switch (direction)
            {
                case Direction4.Right:
                    touchingPoint.X += Width / 2;
                    break;

                case Direction4.Up:
                    touchingPoint.Y += -Height / 2 - 1;
                    break;

                case Direction4.Left:
                    touchingPoint.X += -Width / 2 - 1;
                    break;

                case Direction4.Down:
                    touchingPoint.Y += Height / 2;
                    break;

                default:
                    Debug.Die("Invalid direction for MapEntity::GetTouchingPoint()");
                    break;
            }
            return touchingPoint;
        }

        public Sprite CreateSpriteEx(string animationSetId, string spriteName = "")
        {
            var sprite = CreateSprite(animationSetId, spriteName);
            sprite.EnablePixelCollisions();
            if (IsSuspended)
                sprite.SetSuspended(true);
            return sprite;
        }

        internal Sprite CreateSprite(string animationSetId, string spriteName = "")
        {
            var sprite = Sprite.Create(animationSetId, false);
            _sprites.Add(sprite);
            return sprite;
        }

        public void RemoveSprite(Sprite sprite)
        {
            sprite = sprite ?? Sprite;
            if (_sprites.Contains(sprite))
                _oldSprites.Add(sprite);
            else
                Debug.Die("This sprite does not belong to this entity");
        }

        internal void ClearSprites()
        {
            _oldSprites.AddRange(_sprites);
            _sprites.Clear();
        }

        void ClearOldSprites()
        {
            _sprites.RemoveAll(_oldSprites.Contains);
            _oldSprites.Clear();
        }

        internal virtual void NotifySpriteFrameChanged(Sprite sprite, string animation, int frame)
        {
        }

        internal virtual void NotifySpriteAnimationFinished(Sprite sprite, string animation)
        {
        }

        internal virtual void SetSuspended(bool suspended)
        {
            IsSuspended = suspended;

            if (suspended)
                WhenSuspended = Core.Now;

            foreach (var sprite in _sprites)
                sprite.SetSuspended(suspended || !IsEnabled);

            if (Movement != null)
                Movement.SetSuspended(suspended || !IsEnabled);

            if (IsOnMap)
                Timer.SetEntityTimersSuspended(this, suspended || !IsEnabled);
        }

        internal virtual void Update()
        {
            if (Type == EntityType.Tile)
                throw new InvalidOperationException("Attempt to update a static tile");

            EnableIfNecessary();

            if (_facingEntity?.IsBeingRemoved == true)
                FacingEntity = null;

            // 스프라이트 업데이트
            foreach (var sprite in _sprites)
            {
                sprite.Update();
                if (sprite.HasFrameChanged)
                {
                    if (sprite.ArePixelCollisionsEnabled())
                        CheckCollisionWithDetectors(sprite);

                    NotifySpriteFrameChanged(sprite, sprite.CurrentAnimation, sprite.CurrentFrame);
                    if (sprite.IsAnimationFinished)
                        NotifySpriteAnimationFinished(sprite, sprite.CurrentAnimation);
                }
            }
            ClearOldSprites();

            // 이동 업데이트
            Movement?.Update();
            ClearOldMovements();
        }
        
        void EnableIfNecessary()
        {
            if (!_waitingEnabled)
                return;

            if (IsObstacleFor(Hero) && Overlaps(Hero))
                return;

            IsEnabled = true;
            _waitingEnabled = false;
            // TODO: NotifyEnabled(true);

            if (IsSuspended)
                return;

            Movement?.SetSuspended(false);
            _sprites.Do(s => s.SetSuspended(false));

            if (IsOnMap)
                Timer.SetEntityTimersSuspended(this, false);
        }

        internal virtual void DrawOnMap()
        {
            _sprites.Do(s => Map.DrawSprite(s, GetDisplayedXY()));
        }
        
        internal bool Overlaps(Point point)
        {
            return _boundingBox.Contains(point);
        }

        internal bool Overlaps(int x, int y)
        {
            return _boundingBox.Contains(x, y);
        }

        internal bool Overlaps(Rectangle rectangle)
        {
            return _boundingBox.Overlaps(rectangle);
        }

        internal bool Overlaps(MapEntity other)
        {
            return Overlaps(other.BoundingBox);
        }

        internal bool IsFacingPointIn(Rectangle rectangle)
        {
            return rectangle.Contains(GetFacingPoint());
        }

        internal void SetMovement(Movement movement)
        {
            ClearMovement();
            Movement = movement;

            if (Movement == null)
                return;
             
            Movement.SetEntity(this);

            if (movement.IsSuspended != IsSuspended)
                movement.SetSuspended(IsSuspended || !IsEnabled);
        }

        public void ClearMovement()
        {
            if (Movement == null)
                return;

            Movement.SetEntity(null);
            _oldMovements.Add(Movement);
            Movement = null;
        }

        void ClearOldMovements()
        {
            _oldMovements.Clear();
        }

        internal virtual void NotifyPositionChanged()
        {
            CheckCollisionWithDetectors();
        }

        // 엔티티에게 이동 특성이 변했음을 알리기 위해 Movement 객체에 의해 호출됩니다.
        internal virtual void NotifyMovementChanged()
        {
        }

        internal virtual void NotifyMovementFinished()
        {
        }

        internal virtual void NotifyMovingBy(MapEntity entity)
        {
        }

        internal virtual void NotifyMovedBy(MapEntity entity)
        {
        }

        internal virtual void NotifyLayerChanged()
        {
            if (IsOnMap)
                CheckCollisionWithDetectors();
        }

        internal virtual void NotifyFacingEntityChanged(Detector facingEntity)
        {
        }

        internal static Point DirectionToXyMove(Direction8 direction8)
        {
            return DirectionsToXyMoves[(int)direction8];
        }

        internal void CheckCollisionWithDetectors()
        {
            if (!IsOnMap)
                return; // 초기화 중입니다

            if (!IsEnabled)
                return;

            // 간단한 충돌 검사
            Map.CheckCollisionWithDetectors(this);

            // 픽셀단위 충돌 검사
            foreach (Sprite sprite in _sprites)
                if (sprite.ArePixelCollisionsEnabled())
                    Map.CheckCollisionWithDetectors(this, sprite);
        }

        internal void CheckCollisionWithDetectors(Sprite sprite)
        {
            if (!IsEnabled)
                return;

            Map.CheckCollisionWithDetectors(this, sprite);
        }

        internal virtual void NotifyCollisionWithDestructible(Destructible destructible, CollisionMode collisionMode)
        {
        }

        internal virtual void NotifyCollisionWithChest(Chest chest)
        {
        }

        internal virtual void NotifyCollisionWithBlock(Block block)
        {
        }

        internal virtual void NotifyCollisionWithBomb(Bomb bomb, CollisionMode collisionMode)
        {
        }

        internal virtual void NotifyCollisionWithExplosion(Explosion explosion, Sprite spriteOverlapping)
        {
        }

        internal virtual bool IsObstacleFor(MapEntity other)
        {
            return false;
        }

        internal virtual bool IsObstacleFor(MapEntity other, Rectangle candidatePosition)
        {
            return IsObstacleFor(other);
        }

        internal virtual bool IsDestructibleObstacle(Destructible destructible)
        {
            return !destructible.IsWaitingForRegeneration;
        }

        internal virtual bool IsNpcObstacle(Npc npc)
        {
            return true;
        }

        internal virtual bool IsHeroObstacle(Hero hero)
        {
            return false;
        }

        internal virtual bool IsBlockObstacle(Block block)
        {
            return true;
        }
    }
}
