using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.LowLevel;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    public abstract class Entity : DisposableObject
    {
        public class NamedSprite
        {
            public string Name { get; }
            public Sprite Sprite { get; }
            internal bool Removed { get; set; } = false;

            public NamedSprite(string name, Sprite sprite)
            {
                Name = name;
                Sprite = sprite;
            }
        }

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

        internal bool IsAlignedToGrid => IsAlignedToGridX && IsAlignedToGridY;
        internal bool IsAlignedToGridX => TopLeftX % 8 == 0;
        internal bool IsAlignedToGridY => TopLeftY % 8 == 0;

        internal bool HasSprite => _sprites.Any(s => !s.Removed); 
        internal IEnumerable<Sprite> Sprites => _sprites.Where(s => !s.Removed).Select(s => s.Sprite);
        internal IEnumerable<NamedSprite> NamedSprites => _sprites;
        internal string DefaultSpriteName { get; set; }

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

        readonly List<NamedSprite> _sprites = new List<NamedSprite>();
        readonly List<Movement> _oldMovements = new List<Movement>();

        bool _initialized;
        Point _origin;
        Rectangle _boundingBox;
        Detector _facingEntity;

        protected Entity(string name, Direction4 direction, Layer layer, Point xy, Size size)
        {
            if (size.Width % 8 != 0 || size.Height % 8 != 0)
                throw new ArgumentOutOfRangeException("Invalid entity size: width and height must be multiple of 8");

            Name = name;
            Direction = direction;
            Layer = layer;
            _boundingBox = new Rectangle(xy, size);
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
                IsEnabled = true;

                if (!IsSuspended)
                {
                    Movement?.SetSuspended(false);
                    _sprites.Where(s => !s.Removed).Do(s => s.Sprite.SetSuspended(false));
                    if (IsOnMap)
                        Timer.SetEntityTimersSuspended(this, false);
                }
            }
            else
            {
                IsEnabled = false;

                if (!IsSuspended)
                {
                    Movement?.SetSuspended(true);
                    _sprites.Where(s => !s.Removed).Do(s => s.Sprite.SetSuspended(true));

                    if (IsOnMap)
                        Timer.SetEntityTimersSuspended(this, true);
                }
            }
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
            Sprites.Do(s => s.SetTileset(Map.Tileset));
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
            var direction4 = Direction4.Up; // 기본으로 북쪽
            var sprite = GetSprite();
            if (sprite?.NumDirections == 4)
                direction4 = sprite.CurrentDirection;
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

        public Sprite GetSprite(string spriteName = null)
        {
            if (_sprites.Count <= 0)
                return null;

            string validSpriteName;
            if (spriteName == null)
            {
                if (DefaultSpriteName == null)
                    return Sprites.FirstOrDefault();
                else
                    validSpriteName = DefaultSpriteName;
            }
            else
                validSpriteName = spriteName;

            return _sprites.FirstOrDefault(s => s.Name == validSpriteName && !s.Removed)?.Sprite;
        }

        internal Sprite CreateSprite(string animationSetId, string spriteName = "")
        {
            var sprite = Sprite.Create(animationSetId, false);
            _sprites.Add(new NamedSprite(spriteName, sprite));
            return sprite;
        }

        public bool RemoveSprite(Sprite sprite)
        {
            var namedSprite = _sprites.FirstOrDefault(s => s.Sprite == sprite && !s.Removed);
            if (namedSprite == null)
                return false;

            namedSprite.Removed = true;
            return true;
        }

        internal void ClearSprites()
        {
            _sprites.Do(s => s.Removed = true);
        }

        void ClearOldSprites()
        {
            _sprites.RemoveAll(s => s.Removed);
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

            Sprites.Do(s => s.SetSuspended(suspended && !IsEnabled));
            Movement?.SetSuspended(suspended || !IsEnabled);

            if (IsOnMap)
                Timer.SetEntityTimersSuspended(this, suspended || !IsEnabled);

            if (!suspended)
                CheckCollisionWithDetectors();
        }

        internal virtual void Update()
        {
            if (Type == EntityType.Tile)
                throw new InvalidOperationException("Attempt to update a static tile");

            if (IsBeingRemoved)
                return;
            
            if (_facingEntity?.IsBeingRemoved == true)
                FacingEntity = null;

            // 스프라이트 업데이트
            foreach (var sprite in Sprites)
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
        
        internal virtual void DrawOnMap()
        {
            Sprites.Do(s => Map.DrawSprite(s, GetDisplayedXY()));
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

        internal bool Overlaps(Entity other)
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

        internal virtual void NotifyMovingBy(Entity entity)
        {
        }

        internal virtual void NotifyMovedBy(Entity entity)
        {
        }

        internal virtual void NotifyLayerChanged()
        {
            if (!IsOnMap)
                return;

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
                return; // 초기화 중

            if (!IsEnabled)
                return;

            Map.CheckCollisionWithDetectors(this);

            Sprites.Where(s => s.ArePixelCollisionsEnabled()).Do(s => Map.CheckCollisionWithDetectors(this, s));
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

        internal virtual bool IsObstacleFor(Entity other)
        {
            return false;
        }

        internal virtual bool IsObstacleFor(Entity other, Rectangle candidatePosition)
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
