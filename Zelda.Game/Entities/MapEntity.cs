using System.Collections.Generic;
using Zelda.Game.Lowlevel;
using Zelda.Game.Movements;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    abstract class MapEntity : DisposableObject
    {
        static readonly int DefaultOptimizationDistance = 400;
        static readonly Point[] _directionsToXyMoves = new Point[]
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
        Direction4 _direction;
        MainLoop _mainLoop;
        Point _origin;
        Rectangle _boundingBox;
        int _optimizationDistance = DefaultOptimizationDistance;
        Detector _facingEntity;
        bool _waitingEnabled;

        public Map Map { get; private set; }
        public bool IsOnMap { get { return Map != null; } }
        public bool IsBeingRemoved { get; private set; }
        public bool IsHero { get { return Type == EntityType.Hero; } }
        public bool IsDrawnInYOrder { get; private set; }
        public string Name { get; set; }
        public Layer Layer { get; private set; }
        public Ground GroundBelow { get; private set; }
        
        internal Game Game
        {
            get
            {
                Debug.CheckAssertion(Map != null, "No map was set");
                return Map.Game;
            }
        }
        
        public Direction4 Direction
        {
            get { return _direction; }
            set
            {
                if (value != _direction)
                {
                    _direction = value;
                }
            }
        }

        public Rectangle BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        public int Width
        {
            get { return _boundingBox.Width; }
        }

        public int Height
        {
            get { return _boundingBox.Height; }
        }

        public Size Size
        {
            get { return _boundingBox.Size; }
            set
            {
                Debug.CheckAssertion(value.Width % 8 == 0 && value.Height % 8 == 0,
                    "Invalid entity size: width and height must be multiple of 8");
                _boundingBox.Size = value;
            }
        }

        public int TopLeftX
        {
            get { return _boundingBox.X; }
            set { _boundingBox.X = value; }
        }

        public int TopLeftY
        {
            get { return _boundingBox.Y; }
            set { _boundingBox.Y = value; }
        }

        public Point TopLeftXY
        {
            get { return new Point(TopLeftX, TopLeftY); }
            set { TopLeftX = value.X; TopLeftY = value.Y; }
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

        public Point XY
        {
            get { return new Point(X, Y); }
            set { X = value.X; Y = value.Y; }
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

        public Detector FacingEntity
        {
            get { return _facingEntity; }
            set
            {
                _facingEntity = value;
                NotifyFacingEntityChanged(value);
            }
        }

        public int OptimizationDistance2 { get; private set; }

        public bool IsAlignedToGrid { get { return IsAlignedToGridX && IsAlignedToGridY; } }
        public bool IsAlignedToGridX { get { return TopLeftX % 8 == 0; } }
        public bool IsAlignedToGridY { get { return TopLeftY % 8 == 0; } }
        public bool IsVisible { get; private set; }
        public bool IsEnabled { get; private set; }
        public IEnumerable<Sprite> Sprites { get { return _sprites; } }
        public bool HasSprite { get { return _sprites.Count > 0; } }
        public Sprite Sprite { get { return _sprites[0]; } }
        public bool IsSuspended { get; private set; }
        public Movement Movement { get; private set; }

        public virtual ScriptEntity ScriptEntity { get { return null; } }
        public virtual bool IsDrawnAtItsPosition { get { return true; } }
        public virtual bool CanBeDrawn { get { return true; } }
        public virtual bool CanBeObstacle { get { return true; } }
        public virtual bool IsDetector { get { return false; } }
        public virtual bool HasLayerIndependentCollisions { get { return false; } }
        public virtual bool IsGroundModifier { get { return false; } }
        public virtual Ground ModifiedGround { get { return Ground.Empty; } }

        public virtual bool IsLowWallObstacle { get { return true; } }
        public virtual bool IsShallowWaterObstacle { get { return IsDeepWaterObstacle; } }
        public virtual bool IsDeepWaterObstacle { get { return true; } }
        public virtual bool IsHoleObstacle { get { return true; } }
        public virtual bool IsLavaObstacle { get { return true; } }
        public virtual bool IsPrickleObstacle { get { return true; } }
        public virtual bool IsLadderObstacle { get { return true; } }

        public abstract EntityType Type { get; }

        protected MapEntities Entities
        {
            get
            {
                Debug.CheckAssertion(Map != null, "No map was set");
                return Map.Entities;
            }
        }

        protected CommandsEffects CommandsEffects { get { return Game.CommandsEffects; } }
        protected Equipment Equipment { get { return Game.Equipment; } }
        protected Hero Hero { get { return Entities.Hero; } }
        protected Savegame Savegame { get { return Game.SaveGame; } }
        protected uint WhenSuspended { get; private set; }

        protected MapEntity(string name, Direction4 direction, Layer layer, Point xy, Size size)
        {
            Debug.CheckAssertion(size.Width % 8 == 0 && size.Height % 8 == 0,
                "Invalid entity size: width and height must be multiple of 8");

            Name = name;
            _direction = direction;
            Layer = layer;
            _boundingBox = new Rectangle(xy, size);
            OptimizationDistance2 = DefaultOptimizationDistance * DefaultOptimizationDistance;
            IsVisible = true;
            IsEnabled = true;
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
                    ScriptTimer.SetEntityTimersSuspended(ScriptEntity, true);
            }
            // TODO: NotifyEnabled(false);
        }

        public void RemoveFromMap()
        {
            Map.Entities.RemoveEntity(this);
        }

        public virtual void NotifyBeingRemoved()
        {
            IsBeingRemoved = true;
        }

        public bool IsDrawn()
        {
            bool far = (GetDistanceToCamera2() > OptimizationDistance2) &&
                (_optimizationDistance > 0);
            return IsVisible &&
                (OverlapsCamera() || !far || !!IsDrawnAtItsPosition);
        }

        public void SetDrawnInYOrder(bool drawnInYOrder)
        {
            if (drawnInYOrder != IsDrawnInYOrder)
            {
                IsDrawnInYOrder = drawnInYOrder;
                if (IsOnMap)
                    Entities.SetEntityDrawnInYOrder(this, drawnInYOrder);
            }
        }

        public void SetMap(Map map)
        {
            _mainLoop = map.Game.MainLoop;
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

        public virtual void NotifyMapStarted()
        {
            if (!_initialized)
                FinishInitialization();
        }

        public virtual void NotifyCreating()
        {
        }

        public virtual void NotifyCreated()
        {
        }

        public virtual void NotifyTilesetChanged()
        {
            foreach (Sprite sprite in _sprites)
                sprite.SetTileset(Map.Tileset);
        }

        public void SetLayer(Layer layer)
        {
            Layer = layer;
            NotifyLayerChanged();
        }

        public Point GetDisplayedXY()
        {
            if (Movement == null)
                return XY;

            return Movement.GetDisplayedXY();
        }

        // 엔티티가 바라보는 위치 좌표를 반환합니다.
        // 스프라이트가 있다면 스프라이트의 방향에 기반한 위치입니다.
        // 스프라이트가 없거나 스프라이트가 4방향이 아니라면 이동을 고려한 위치입니다.
        // 이동도 없다면, 북쪽을 바라보는 것으로 가정합니다.
        public virtual Point GetFacingPoint()
        {
            Direction4 direction4 = Direction4.Up; // 기본으로 북쪽
            if (HasSprite && Sprite.NumDirections == 4)
                direction4 = Sprite.CurrentDirection;
            else if (Movement != null)
                direction4 = Movement.GetDisplayedDirection4();

            return GetTouchingPoint(direction4);
        }

        public Point GetTouchingPoint(Direction4 direction)
        {
            Point touchingPoint = CenterPoint;
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

        public Sprite CreateSprite(string animationSetId, bool enablePixelCollisions = false)
        {
            Sprite sprite = new Sprite(animationSetId);

            if (enablePixelCollisions)
                sprite.EnablePixelCollisions();

            _sprites.Add(sprite);
            return sprite;
        }

        public void RemoveSprite(Sprite sprite)
        {
            if (_sprites.Contains(sprite))
                _oldSprites.Add(sprite);
            else
                Debug.Die("This sprite does not belong to this entity");
        }

        public void ClearSprites()
        {
            _oldSprites.AddRange(_sprites);
            _sprites.Clear();
        }

        private void ClearOldSprites()
        {
            _sprites.RemoveAll(_oldSprites.Contains);
            _oldSprites.Clear();
        }

        public virtual void NotifySpriteFrameChanged(Sprite sprite, string animation, int frame)
        {
        }

        public virtual void NotifySpriteAnimationFinished(Sprite sprite, string animation)
        {
        }

        public virtual void SetSuspended(bool suspended)
        {
            IsSuspended = suspended;

            if (suspended)
                WhenSuspended = Engine.Now;

            foreach (var sprite in _sprites)
                sprite.SetSuspended(suspended || !IsEnabled);

            if (Movement != null)
                Movement.SetSuspended(suspended || !IsEnabled);

            if (IsOnMap)
                ScriptTimer.SetEntityTimersSuspended(ScriptEntity, suspended || !IsEnabled);
        }

        public virtual void Update()
        {
            Debug.CheckAssertion(Type != EntityType.Tile, "Attempt to update a static tile");

            EnableIfNecessary();

            if (_facingEntity != null && _facingEntity.IsBeingRemoved)
                FacingEntity = null;

            // 스프라이트 업데이트
            foreach (Sprite sprite in _sprites)
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
            if (Movement != null)
                Movement.Update();
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

            if (Movement != null)
                Movement.SetSuspended(false);

            foreach (var sprite in _sprites)
                sprite.SetSuspended(false);

            if (IsOnMap)
                ScriptTimer.SetEntityTimersSuspended(ScriptEntity, false);
        }

        public virtual void DrawOnMap()
        {
            if (!IsDrawn())
                return;

            foreach (Sprite sprite in _sprites)
                Map.DrawSprite(sprite, GetDisplayedXY());
        }
        
        public bool Overlaps(Point point)
        {
            return _boundingBox.Contains(point);
        }

        public bool Overlaps(int x, int y)
        {
            return _boundingBox.Contains(x, y);
        }

        public bool Overlaps(Rectangle rectangle)
        {
            return _boundingBox.Overlaps(rectangle);
        }

        public bool Overlaps(MapEntity other)
        {
            return Overlaps(other.BoundingBox);
        }

        // 엔티티의 'origin' 지점과 맵 가시 영역의 중점사이의 거리의 제곱을 얻습니다
        public int GetDistanceToCamera2()
        {
            return Geometry.GetDistance2(XY, Map.CameraPosition.Center);
        }

        // 엔티티의 바운딩 박스 혹은 스프라이트가 맵의 가시영역과 겹치는지를 확인합니다
        public bool OverlapsCamera()
        {
            Rectangle cameraPosition = Map.CameraPosition;
            if (_boundingBox.Overlaps(cameraPosition))
                return true;

            // TODO: 스프라이트와 겹치는지 확인

            return false;
        }

        public bool IsFacingPointIn(Rectangle rectangle)
        {
            return rectangle.Contains(GetFacingPoint());
        }

        public void SetMovement(Movement movement)
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
            if (Movement != null)
            {
                Movement.SetEntity(null);
                Movement.ScriptMovement = null;
                _oldMovements.Add(Movement);
                Movement = null;
            }
        }

        void ClearOldMovements()
        {
            _oldMovements.Clear();
        }

        public virtual void NotifyPositionChanged()
        {
            CheckCollisionWithDetectors();
        }

        // 엔티티에게 이동 특성이 변했음을 알리기 위해 Movement 객체에 의해 호출됩니다.
        public virtual void NotifyMovementChanged()
        {
        }

        public virtual void NotifyMovementFinished()
        {
        }

        public virtual void NotifyMovingBy(MapEntity entity)
        {
        }

        public virtual void NotifyMovedBy(MapEntity entity)
        {
        }

        public virtual void NotifyLayerChanged()
        {
            if (IsOnMap)
                CheckCollisionWithDetectors();
        }

        public virtual void NotifyFacingEntityChanged(Detector facingEntity)
        {
        }

        public static Point DirectionToXyMove(Direction8 direction8)
        {
            return _directionsToXyMoves[(int)direction8];
        }

        public void CheckCollisionWithDetectors()
        {
            if (!IsOnMap)
                return; // 초기화 중입니다

            if (!IsEnabled)
                return;

            if (GetDistanceToCamera2() > OptimizationDistance2 &&
                _optimizationDistance > 0)
                return; // 가시 영역으로부터 멀리 떨어진 엔티티들은 체크하지 않습니다

            // 간단한 충돌 검사
            Map.CheckCollisionWithDetectors(this);

            // 픽셀단위 충돌 검사
            foreach (Sprite sprite in _sprites)
                if (sprite.ArePixelCollisionsEnabled())
                    Map.CheckCollisionWithDetectors(this, sprite);
        }

        public void CheckCollisionWithDetectors(Sprite sprite)
        {
            if (!IsEnabled)
                return;

            if (GetDistanceToCamera2() > OptimizationDistance2 &&
                _optimizationDistance > 0)
                return;

            Map.CheckCollisionWithDetectors(this, sprite);
        }

        public virtual void NotifyCollisionWithDestructible(Destructible destructible, CollisionMode collisionMode)
        {
        }

        public virtual void NotifyCollisionWithChest(Chest chest)
        {
        }

        public virtual void NotifyCollisionWithBlock(Block block)
        {
        }

        public virtual void NotifyCollisionWithBomb(Bomb bomb, CollisionMode collisionMode)
        {
        }

        public virtual void NotifyCollisionWithExplosion(Explosion explosion, Sprite spriteOverlapping)
        {
        }

        public virtual bool IsObstacleFor(MapEntity other)
        {
            return false;
        }

        public virtual bool IsObstacleFor(MapEntity other, Rectangle candidatePosition)
        {
            return IsObstacleFor(other);
        }

        public virtual bool IsDestructibleObstacle(Destructible destructible)
        {
            return !destructible.IsWaitingForRegeneration;
        }

        public virtual bool IsNpcObstacle(Npc npc)
        {
            return true;
        }

        public virtual bool IsHeroObstacle(Hero hero)
        {
            return false;
        }

        public virtual bool IsBlockObstacle(Block block)
        {
            return true;
        }
    }
}
