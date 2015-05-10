﻿using System;
using System.Collections.Generic;
using Zelda.Game.Engine;
using Zelda.Game.Movements;

namespace Zelda.Game.Entities
{
    abstract class MapEntity : DisposableObject
    {
        MainLoop _mainLoop;

        #region 생성과 소멸
        protected MapEntity(string name, Direction4 direction, Layer layer, Point xy, Size size)
        {
            Debug.CheckAssertion(size.Width % 8 == 0 && size.Height % 8 == 0,
                "Invalid entity size: width and height must be multiple of 8");

            Name = name;
            _direction = direction;
            _layer = layer;
            _boundingBox = new Rectangle(xy, size);
        }

        bool _initialized;

        protected override void OnDispose(bool disposing)
        {
            ClearSprites();
            ClearOldSprites();
            ClearMovement();
            ClearOldMovements();
        }

        public void RemoveFromMap()
        {
            Map.Entities.RemoveEntity(this);
        }

        public bool IsBeingRemoved { get; private set; }

        public virtual void NotifyBeingRemoved()
        {
            IsBeingRemoved = true;
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

        public virtual bool IsDetector
        {
            get { return false; }
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
        Layer _layer;
        public Layer Layer
        {
            get { return _layer; }
        }

        public void SetLayer(Layer layer)
        {
            _layer = layer;
            NotifyLayerChanged();
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

        public Point GetDisplayedXY()
        {
            if (_movement == null)
                return XY;

            return _movement.GetDisplayedXY();
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

        public Point CenterPoint
        {
            get { return BoundingBox.Center; }
        }
        #endregion

        #region 속성
        Direction4 _direction;
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

        protected CommandsEffects CommandsEffects
        {
            get { return Game.CommandsEffects; }
        }

        protected Equipment Equipment
        {
            get { return Game.Equipment; }
        }

        protected Hero Hero
        {
            get { return Entities.Hero; }
        }

        protected Savegame Savegame
        {
            get { return Game.SaveGame; }
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

            if (_facingEntity != null && _facingEntity.IsBeingRemoved)
                FacingEntity = null;

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
                _map.DrawSprite(sprite, GetDisplayedXY());
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

        public bool IsFacingPointIn(Rectangle rectangle)
        {
            return rectangle.Contains(GetFacingPoint());
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

        public virtual void NotifyPositionChanged()
        {
            CheckCollisionWithDetectors();
        }

        // 엔티티에게 이동 특성이 변했음을 알리기 위해 Movement 객체에 의해 호출됩니다.
        public virtual void NotifyMovementChanged()
        {
        }

        public virtual void NotifyLayerChanged()
        {
            if (IsOnMap)
                CheckCollisionWithDetectors();
        }

        Detector _facingEntity;
        public Detector FacingEntity
        {
            get { return _facingEntity; }
            set
            {
                _facingEntity = value;
                NotifyFacingEntityChanged(value);
            }
        }

        public virtual void NotifyFacingEntityChanged(Detector facingEntity)
        {
        }

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

        public static Point DirectionToXyMove(Direction8 direction8)
        {
            return _directionsToXyMoves[(int)direction8];
        }
        #endregion

        #region 충돌
        public virtual bool HasLayerIndependentCollisions
        {
            get { return false; }
        }

        public void CheckCollisionWithDetectors()
        {
            if (!IsOnMap)
                return; // 초기화 중입니다

            if (GetDistanceToCamera2() > _optimizationDistance2 &&
                _optimizationDistance > 0)
                return; // 가시 영역으로부터 멀리 떨어진 엔티티들은 체크하지 않습니다

            // 간단한 충돌 검사
            _map.CheckCollisionWithDetectors(this);
        }

        public void CheckCollisionWithDetectors(Sprite sprite)
        {
            if (GetDistanceToCamera2() > _optimizationDistance2 &&
                _optimizationDistance > 0)
                return;

            _map.CheckCollisionWithDetectors(this, sprite);
        }

        public virtual void NotifyCollisionWithDestructible(Destructible destructible, CollisionMode collisionMode)
        {
        }

        public virtual void NotifyCollisionWithChest(Chest chest)
        {
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
