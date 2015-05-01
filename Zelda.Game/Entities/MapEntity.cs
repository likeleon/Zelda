using System;
using System.Collections.Generic;
using Zelda.Game.Engine;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    abstract class MapEntity : IDisposable
    {
        MainLoop _mainLoop;

        #region 생성과 소멸
        protected MapEntity(string name, int direction, Layer layer, Point xy, Size size)
        {
            Debug.CheckAssertion(size.Width % 8 == 0 && size.Height % 8 == 0,
                "Invalid entity size: width and height must be multiple of 8");

            Name = name;
            _direction = direction;
            _layer = layer;
            _boundingBox = new Rectangle(xy, size);
        }

        bool _initialized;
        bool _disposed;

        ~MapEntity()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            ClearSprites();
            ClearOldSprites();
            ClearMovement();
            ClearOldMovements();
            _disposed = true;
        }
        #endregion

        #region 타입
        public abstract EntityType Type { get; }

        public virtual bool IsDrawnAtItsPosition
        {
            get { return true; }
        }

        public virtual bool CanBeDrawn
        {
            get { return true; }
        }

        private bool _drawnInYOrder;
        public bool IsDrawnInYOrder
        {
            get { return _drawnInYOrder; }
        }

        public bool IsDrawn()
        {
            bool far = (GetDistanceToCamera2() > _optimizationDistance2) &&
                (_optimizationDistance > 0);
            return IsVisible &&
                (OverlapsCamera() || !far || !!IsDrawnAtItsPosition);
        }

        public void SetDrawnInYOrder(bool drawnInYOrder)
        {
            if (drawnInYOrder != _drawnInYOrder)
            {
                _drawnInYOrder = drawnInYOrder;
                if (IsOnMap)
                    Entities.SetEntityDrawnInYOrder(this, drawnInYOrder);
            }
        }

        public virtual bool CanBeObstacle
        {
            get { return true; }
        }
        #endregion

        #region 속성들
        public string Name { get; set; }
        #endregion

        #region 맵에 추가
        Map _map;
        public Map Map
        {
            get { return _map; }
        }

        public void SetMap(Map map)
        {
            _mainLoop = map.Game.MainLoop;
            _map = map;
            if (Game.CurrentMap == map)
                NotifyTilesetChanged();

            _groundBelow = Ground.Empty;

            if (!_initialized && _map.IsLoaded)
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

        public bool IsOnMap
        {
            get { return _map != null; }
        }

        public Game Game
        {
            get
            {
                Debug.CheckAssertion(Map != null, "No map was set");
                return _map.Game;
            }
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
                sprite.SetTileset(_map.Tileset);
        }
        #endregion

        #region 맵에서의 위치
        readonly Layer _layer;
        public Layer Layer
        {
            get { return _layer; }
        }

        Ground _groundBelow;
        public Ground GroundBelow
        {
            get { return _groundBelow; }
        }

        Rectangle _boundingBox;
        public Rectangle BoundingBox
        {
            get { return _boundingBox; }
        }

        public int Width
        {
            get { return _boundingBox.Width; }
        }

        public int Height
        {
            get { return _boundingBox.Height; }
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

        Point _origin;
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

        private static int DefaultOptimizationDistance = 400;

        int _optimizationDistance = DefaultOptimizationDistance;
        public int OptimizationDistance
        {
            get { return _optimizationDistance; }
            set
            {
                _optimizationDistance = value;
                _optimizationDistance2 = value * value;
            }
        }

        int _optimizationDistance2 = DefaultOptimizationDistance * DefaultOptimizationDistance;
        public int OptimizationDistance2
        {
            get { return _optimizationDistance2; }
        }

        public Point DisplayedXY
        {
            get
            {
                if (_movement == null)
                    return XY;
                
                return _movement.DisplayedXY;
            }
        }
        #endregion

        #region 속성
        int _direction;
        public int Direction
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
        #endregion

        #region 스프라이트
        bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        readonly List<Sprite> _sprites = new List<Sprite>();
        public IEnumerable<Sprite> Sprites
        {
            get { return _sprites; }
        }

        public bool HasSprite
        {
            get { return _sprites.Count > 0; }
        }

        public Sprite Sprite
        {
            get { return _sprites[0]; }
        }

        public Sprite CreateSprite(string animationSetId)
        {
            Sprite sprite = new Sprite(animationSetId);
            _sprites.Add(sprite);
            return sprite;
        }

        readonly List<Sprite> _oldSprites = new List<Sprite>();

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
        #endregion

        #region 게임 오브젝트 접근용 (편의성)
        protected MapEntities Entities
        {
            get
            {
                Debug.CheckAssertion(_map != null, "No map was set");
                return _map.Entities;
            }
        }
        #endregion

        #region 게임 루프
        bool _suspended;
        public bool IsSuspended
        {
            get { return _suspended; }
        }

        uint _whenSuspended;
        protected uint WhenSuspended
        {
            get { return _whenSuspended; }
        }

        public virtual void SetSuspended(bool suspended)
        {
            _suspended = suspended;

            if (suspended)
                _whenSuspended = EngineSystem.Now;

            foreach (Sprite sprite in _sprites)
                sprite.SetSuspended(suspended);

            if (_movement != null)
                _movement.SetSuspended(suspended);
        }

        public virtual void Update()
        {
            Debug.CheckAssertion(Type != EntityType.Tile, "Attempt to update a static tile");

            // 스프라이트 업데이트
            foreach (Sprite sprite in _sprites)
                sprite.Update();
            ClearOldSprites();

            // 이동 업데이트
            if (_movement != null)
                _movement.Update();
            ClearOldMovements();
        }

        public virtual void DrawOnMap()
        {
            if (!IsDrawn())
                return;

            foreach (Sprite sprite in _sprites)
                _map.DrawSprite(sprite, DisplayedXY);
        }
        #endregion

        #region 기하
        public bool Overlaps(Rectangle rectangle)
        {
            return _boundingBox.Overlaps(rectangle);
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
        #endregion

        #region 이동
        Movement _movement;
        public Movement Movement
        {
            get { return _movement; }
        }

        public void SetMovement(Movement movement)
        {
            ClearMovement();
            _movement = movement;

            if (_movement != null)
            {
                _movement.SetEntity(this);

                if (movement.IsSuspended != _suspended)
                    movement.SetSuspended(_suspended);
            }
        }

        readonly List<Movement> _oldMovements = new List<Movement>();

        public void ClearMovement()
        {
            if (_movement != null)
            {
                _movement.SetEntity(null);
                _movement.ScriptMovement = null;
                _oldMovements.Add(_movement);
                _movement = null;
            }
        }

        void ClearOldMovements()
        {
            _oldMovements.Clear();
        }

        // 엔티티에게 이동 특성이 변했음을 알리기 위해 Movement 객체에 의해 호출됩니다.
        public virtual void NotifyMovementChanged()
        {
        }
        #endregion

        #region 충돌
        public virtual bool HasLayerIndependentCollisions
        {
            get { return false; }
        }

        public virtual bool IsLowWallObstacle
        {
            get { return true; }
        }

        public virtual bool IsShallowWaterObstacle
        {
            get { return IsDeepWaterObstacle; }
        }

        public virtual bool IsDeepWaterObstacle
        {
            get { return true; }
        }

        public virtual bool IsHoleObstacle
        {
            get { return true; }
        }

        public virtual bool IsLavaObstacle
        {
            get { return true; }
        }

        public virtual bool isPrickleObstacle
        {
            get { return true; }
        }

        public virtual bool IsLadderObstacle
        {
            get { return true; }
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
        #endregion
    }
}
