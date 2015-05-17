using System;
using Zelda.Game.Engine;
using Zelda.Game.Entities;
using ScriptContext = Zelda.Game.Script.ScriptContext;

namespace Zelda.Game
{
    class Map
    {
        #region 생성
        public Map(string id)
        {
            _id = id;

            _cameraPosition = new Rectangle(new Point(), Video.ModSize);
        }
        #endregion

        #region 맵 속성들
        readonly string _id;
        public string Id
        {
            get { return _id; }
        }

        string _tilesetId;
        public string TilesetId
        {
            get { return _tilesetId; }
        }

        Tileset _tileset;
        public Tileset Tileset
        {
            get { return _tileset; }
        }

        string _musicId;
        public string MusicId
        {
            get { return _musicId; }
        }

        string _world;
        public bool HasWorld
        {
            get { return String.IsNullOrEmpty(_world); }
        }

        public string World
        {
            get { return _world; }
        }

        int _floor = MapData.NoFloor;
        public int Floor
        {
            get { return _floor; }
        }

        Rectangle _location;
        public Rectangle Location
        {
            get { return _location; }
        }

        public Size Size
        {
            get { return _location.Size; }
        }

        public int Width
        {
            get { return _location.Width; }
        }

        public int Height
        {
            get { return _location.Height; }
        }

        int _width8;
        public int Width8
        {
            get { return _width8; }
        }

        int _height8;
        public int Height8
        {
            get { return _height8; }
        }
        #endregion

        #region 현재 도착 위치
        string _destinationName;
        public string DestinationName
        {
            get { return _destinationName; }
            set { _destinationName = value; }
        }

        public Destination GetDestination()
        {
            if (_destinationName == "_same" || _destinationName.SafeSubstring(0, 5) == "_side")
                return null;

            Debug.CheckAssertion(IsLoaded, "This map is not loaded");

            Destination destination = null;
            if (!String.IsNullOrEmpty(_destinationName))
            {
                MapEntity entity = _entities.FindEntity(_destinationName);
                if (entity == null || entity.Type != EntityType.Destination)
                    Debug.Error("Map '{0}': No such destination: '{1}'".F(_id, _destinationName));
                else
                    destination = entity as Destination;
            }

            return destination ?? _entities.DefaultDestination;
        }
        #endregion

        #region 로드
        Game _game;
        public Game Game
        {
            get { return _game; }
        }

        bool _loaded;
        public bool IsLoaded
        {
            get { return _loaded; }
        }

        public void Load(Game game)
        {
            _visibleSurface = Surface.Create(Video.ModSize);
            _visibleSurface.SoftwareDestination = false;

            _backgroundSurface = Surface.Create(Video.ModSize);
            _backgroundSurface.SoftwareDestination = false;

            LoadMapData(game);

            BuildBackgroundSurface();
            BuildForegroundSurface();

            _loaded = true;
        }

        public void Unload()
        {
            if (_loaded)
            {
                _tileset = null;
                _visibleSurface = null;
                _backgroundSurface = null;
                _foregroundSurface = null;
                _entities = null;

                _loaded = false;
            }
        }

        // 맵 데이터 파일을 읽습니다.
        void LoadMapData(Game game)
        {
            MapData data = new MapData();
            string fileName = "maps/" + _id + ".xml";
            bool success = data.ImportFromModFile(fileName);

            if (!success)
                Debug.Die("Failed to load map data file '{0}'".F(fileName));

            // 읽어낸 데이터로 맵을 초기화합니다
            _game = game;
            _location = new Rectangle(data.Location, data.Size);
            _width8 = data.Size.Width / 8;
            _height8 = data.Size.Height / 8;
            _musicId = data.MusicId;
            _world = data.World;
            _floor = data.Floor;
            _tilesetId = data.TilesetId;
            _tileset = new Tileset(data.TilesetId);
            _tileset.Load();

            _entities = new MapEntities(game, this);

            // 엔티티들을 생성합니다
            for (int layer = 0; layer < (int)Layer.Count; ++layer)
            {
                for (int i = 0; i < data.GetNumEntities((Layer)layer); ++i)
                {
                    EntityData entityData = data.GetEntity(new EntityIndex((Layer)layer, i));
                    EntityType type = entityData.Type;
                    if (!type.CanBeStoredInMapFile())
                        Debug.Error("Illegal entity type in map file: " + type);

                    ScriptContext.CreateMapEntityFromData(this, entityData);
                }
            }
        }

        void BuildBackgroundSurface()
        {
            if (_tileset != null)
            {
                _backgroundSurface.Clear();
                _backgroundSurface.FillWithColor(_tileset.BackgroundColor);
            }
        }

        void BuildForegroundSurface()
        {
            _foregroundSurface = null;

            int screenWidth = _visibleSurface.Width;
            int screenHeight = VisibleSurface.Height;

            // 맵이 스크린 크기에 비해 작다면, 맵 외부에 검정 바를 위치시킵니다
            if (Width >= screenWidth && Height >= screenHeight)
                return; // 해당 사항이 없습니다

            _foregroundSurface = Surface.Create(_visibleSurface.Size);

            if (Width < screenWidth)
            {
                int barWidth = (screenWidth - Width) / 2;
                Rectangle dstPosition = new Rectangle(0, 0, barWidth, screenHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
                dstPosition.X = barWidth + Width;
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
            }

            if (Height < screenHeight)
            {
                int barHeight = (screenHeight - Height) / 2;
                Rectangle dstPosition = new Rectangle(0, 0, screenWidth, barHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
                dstPosition.Y = barHeight + Height;
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
            }
        }
        #endregion

        #region 엔티티들
        MapEntities _entities;
        public MapEntities Entities
        {
            get { return _entities; }
        }
        #endregion

        #region 메인 루프
        Surface _backgroundSurface;
        Surface _foregroundSurface;

        void DrawBackground()
        {
            _backgroundSurface.Draw(_visibleSurface);
        }

        void DrawForeground()
        {
            if (_foregroundSurface != null)
                _foregroundSurface.Draw(_visibleSurface);
        }

        public void Draw()
        {
            if (_loaded)
            {
                DrawBackground();
                _entities.Draw();
                DrawForeground();
            }
        }

        public void DrawSprite(Sprite sprite, Point xy)
        {
            DrawSprite(sprite, xy.X, xy.Y);
        }

        public void DrawSprite(Sprite sprite, int x, int y)
        {
            sprite.Draw(VisibleSurface, x - _cameraPosition.X, y - _cameraPosition.Y);
        }

        public void Update()
        {
            CheckSuspended();

            TilePattern.Update();
            _entities.Update();
        }

        bool _suspended;
        public bool IsSuspended
        {
            get { return _suspended; }
        }

        public void CheckSuspended()
        {
            bool gameSuspended = _game.IsSuspended;
            if (_suspended != gameSuspended)
                SetSuspended(gameSuspended);
        }

        void SetSuspended(bool suspended)
        {
            _suspended = suspended;
            _entities.SetSuspended(suspended);
        }
        #endregion

        #region 시작
        bool _started;
        public bool IsStarted
        {
            get { return _started; }
        }

        public void Start()
        {
            _started = true;
            _visibleSurface.Opacity = 255;

            Music.Play(_musicId, true);
            _entities.NotifyMapStarted();
        }
        #endregion

        #region 현재 목표 지점
        public int GetDestinationSide()
        {
            if (_destinationName.SafeSubstring(0, 5) == "_side")
            {
                int destinationSide = _destinationName[5] - '0';
                return destinationSide;
            }
            return -1;
        }
        #endregion

        #region 카메라
        Surface _visibleSurface;
        public Surface VisibleSurface
        {
            get { return _visibleSurface; }
        }

        readonly Rectangle _cameraPosition;
        public Rectangle CameraPosition
        {
            get { return _cameraPosition; }
        }
        #endregion

        #region 장애물과의 충돌 (실제 이동 전에)
        public bool TestCollisionWithBorder(int x, int y)
        {
            return (x < 0 || y < 0 || x >= _location.Width || y >= _location.Height);
        }

        public bool TestCollisionWithBorder(Point point)
        {
            return TestCollisionWithBorder(point.X, point.Y);
        }

        // 특정 포인트가 맵의 그라운드와 충돌하는지를 체크합니다.
        // 그라운드는 타일 혹은 타일을 변화시키는 것들(예를 들면, 다이나믹 타일이나 파괴 가능한 아이템)입니다.
        // 포인트가 맵 외부일 경우에도 true를 반환합니다.
        public bool TestCollisionWithGround(Layer layer, int x, int y, MapEntity entityToCheck, ref bool foundDiagonalWall)
        {
            bool onObstacle = false;
            int xInTile = 0, yInTile = 0;

            if (TestCollisionWithBorder(x, y))
                return true;

            // 포인트 위치에서의 그라운드 속성을 얻습니다
            Ground ground = GetGround(layer, x, y);

            switch (ground)
            {
                case Ground.Empty:
                case Ground.Traversable:
                case Ground.Grass:
                case Ground.Ice:
                    onObstacle = false;
                    break;

                case Ground.Wall:
                    onObstacle = true;
                    break;
                    
                case Ground.WallTopRight:
                case Ground.WallTopRightWater:
                    // 상단 우측이 장애물로, 이 안에서 다시 체크해야 합니다
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile <= xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.WallTopLeft:
                case Ground.WallTopLeftWater:
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile <= 7 - xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.WallBottomLeft:
                case Ground.WallBottomLeftWater:
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile >=  xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.WallBottomRight:
                case Ground.WallBottomRightWater:
                    xInTile = x & 7;
                    yInTile = y & 7;
                    onObstacle = yInTile >= 7 - xInTile;
                    foundDiagonalWall = true;
                    break;

                case Ground.LowWall:
                    onObstacle = entityToCheck.IsLowWallObstacle;
                    break;

                case Ground.ShallowWater:
                    onObstacle = entityToCheck.IsShallowWaterObstacle;
                    break;

                case Ground.DeepWater:
                    onObstacle = entityToCheck.IsDeepWaterObstacle;
                    break;

                case Ground.Hole:
                    onObstacle = entityToCheck.IsHoleObstacle;
                    break;

                case Ground.Lava:
                    onObstacle = entityToCheck.IsLavaObstacle;
                    break;

                case Ground.Prickle:
                    onObstacle = entityToCheck.IsPrickleObstacle;
                    break;

                case Ground.Ladder:
                    onObstacle = entityToCheck.IsLadderObstacle;
                    break;
            }

            return onObstacle;
        }

        public bool TestCollisionWithObstacles(Layer layer, Rectangle collisionBox, MapEntity entityToCheck)
        {
            // 이 함수는 매우 자주 불리우며, 성능상의 이유로 충돌 박스의 경계선만을 체크합니다.

            // 터레인과의 퉁돌 (에를 들어 타일 혹은 타일을 변화시키는 동적 엔티티)
            int x1 = collisionBox.X;
            int x2 = x1 + collisionBox.Width - 1;
            int y1 = collisionBox.Y;
            int y2 = y1 + collisionBox.Height - 1;

            // 사각형 꼭지점들만을 먼저 체크합니다. 
            // 충돌 박스가 적어도 8x8 픽셀이기 때문에, 대칭 타입을 제외한 모든 터레인은 이 체크만으로도 충분합니다.
            bool foundDiagonalWall = false;
            for (int x = x1; x <= x2; x += 8)
            {
                if (TestCollisionWithGround(layer, x, y1, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x, y2, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x + 7, y1, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x + 7, y2, entityToCheck, ref foundDiagonalWall))
                {
                    return true;
                }
            }

            for (int y = y1; y <= y2; y += 8)
                if (TestCollisionWithGround(layer, x1, y, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x2, y, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x1, y + 7, entityToCheck, ref foundDiagonalWall) ||
                    TestCollisionWithGround(layer, x1, y + 7, entityToCheck, ref foundDiagonalWall))
                    return true;

            // 완전한 체크가 필요합니다
            // 꼭지점들만 체크할 경우 'V'와 같이 각도가 큰 벽의 경우 통과가 가능하게 때문에, 충돌 박스의 경계선 모두를 체크해야 합니다.
            if (foundDiagonalWall)
            {
                for (int x = x1; x <= x2; ++x)
                {
                    if (TestCollisionWithGround(layer, x, y1, entityToCheck, ref foundDiagonalWall) ||
                        TestCollisionWithGround(layer, x, y2, entityToCheck, ref foundDiagonalWall))
                    {
                        return true;
                    }
                }

                for (int y = y1; y <= y2; ++y)
                {
                    if (TestCollisionWithGround(layer, x1, y, entityToCheck, ref foundDiagonalWall) ||
                        TestCollisionWithGround(layer, x2, y, entityToCheck, ref foundDiagonalWall))
                    {
                        return true;
                    }
                }
            }

            // 터레인과는 충돌이 없습니다. 다이나믹 엔티티들과의 충돌을 검사합니다.
            return TestCollisionWithEntities(layer, collisionBox, entityToCheck);
        }

        public bool TestCollisionWithEntities(Layer layer, Rectangle collisionBox, MapEntity entityToCheck)
        {
            var obstacleEntities = _entities.GetObstacleEntities(layer);
            foreach (MapEntity entity in obstacleEntities)
            {
                if (entity != entityToCheck &&
                    entity.Overlaps(collisionBox) &&
                    entity.IsObstacleFor(entityToCheck, collisionBox))
                {
                    return true;
                }
            }
            
            return false;
        }

        public Ground GetGround(Layer layer, int x, int y)
        {
            // TODO: 그라운드를 변화시키는 다이나믹 엔티티들 처리
            return _entities.GetTileGround(layer, x, y);
        }
        #endregion

        #region 디텍터들과의 충돌 (이동 후에 체크)
        public void CheckCollisionWithDetectors(MapEntity entity)
        {
            if (_suspended)
                return;

            foreach (Detector detector in _entities.Detectors)
            {
                if (!detector.IsBeingRemoved)
                    detector.CheckCollision(entity);
            }
        }

        public void CheckCollisionWithDetectors(MapEntity entity, Sprite sprite)
        {
            if (_suspended)
                return;

            foreach (Detector detector in _entities.Detectors)
            {
                if (!detector.IsBeingRemoved)
                    detector.CheckCollision(entity, sprite);
            }
        }

        public void CheckCollisionFromDetector(Detector detector)
        {
            if (_suspended)
                return;

            detector.CheckCollision(Entities.Hero);

            foreach (MapEntity entity in _entities.Entities)
            {
                if (!entity.IsBeingRemoved)
                    detector.CheckCollision(entity);
            }
        }
        #endregion
    }
}
