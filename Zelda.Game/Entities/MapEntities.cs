using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game.Entities
{
    // 맵 컨텐츠를 관리합니다 (타일, 주인공, 적과 같은 맵 엔티티들을 저장하고 관리)
    public class MapEntities
    {
        readonly Game _game;
        readonly Map _map;
        readonly int _mapWidth8;
        readonly int _mapHeight8;
        readonly int _tilesGridSize;
        readonly NonAnimatedRegions[] _nonAnimatedRegions = new NonAnimatedRegions[(int)Layer.NumLayer];
        readonly List<Tile>[] _tilesInAnimatedRegions = new List<Tile>[(int)Layer.NumLayer];
        readonly List<Entity>[] _obstacleEntities = new List<Entity>[(int)Layer.NumLayer];
        readonly Dictionary<string, Entity> _namedEntities = new Dictionary<string, Entity>();
        readonly List<Entity>[] _groundModifiers = new List<Entity>[(int)Layer.NumLayer];
        readonly List<Entity>[] _entitiesDrawnFirst = new List<Entity>[(int)Layer.NumLayer];
        readonly List<Entity>[] _entitiesDrawnYOrder = new List<Entity>[(int)Layer.NumLayer];
        readonly List<Entity> _entitiesToAdd = new List<Entity>();
        readonly List<Entity> _entitiesToRemove = new List<Entity>();
        readonly List<Detector> _detectors = new List<Detector>();
        readonly List<Entity> _allEntities = new List<Entity>();
        readonly Ground[,] _tilesGround;

        internal Hero Hero { get; private set; }
        internal Destination DefaultDestination { get; private set; }
        internal IEnumerable<Entity> Entities { get { return _allEntities; } }
        internal IEnumerable<Detector> Detectors { get { return _detectors; } }

        internal MapEntities(Game game, Map map)
        {
            _game = game;
            _map = map;
            _mapWidth8 = _map.Width8;
            _mapHeight8 = _map.Height8;
            _tilesGridSize = _mapWidth8 * _mapHeight8;
            Hero = _game.Hero;

            _tilesGround = new Ground[(int)Layer.NumLayer, _tilesGridSize];
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
            {
                Ground initialGround = ((Layer)layer == Layer.Low) ? Ground.Traversable : Ground.Empty;
                for (int i = 0; i < _tilesGridSize; ++i)
                    _tilesGround[layer, i] = initialGround;

                _nonAnimatedRegions[layer] = new NonAnimatedRegions(_map, (Layer)layer);
                _tilesInAnimatedRegions[layer] = new List<Tile>();
                _groundModifiers[layer] = new List<Entity>();
                _entitiesDrawnFirst[layer] = new List<Entity>();
                _entitiesDrawnYOrder[layer] = new List<Entity>();
                _obstacleEntities[layer] = new List<Entity>();
            }

            Layer heroLayer = Hero.Layer;
            _obstacleEntities[(int)heroLayer].Add(Hero);
            _entitiesDrawnYOrder[(int)heroLayer].Add(Hero);
            _namedEntities[Hero.Name] = Hero;
        }

        internal void CreateEntities(MapData data)
        {
            // 엔티티들을 생성합니다
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
            {
                for (int i = 0; i < data.GetNumEntities((Layer)layer); ++i)
                {
                    var entityData = data.GetEntity(new EntityIndex((Layer)layer, i));
                    var type = entityData.Type;
                    if (!type.CanBeStoredInMapFile())
                        throw new InvalidOperationException("Illegal entity type in map file: " + type);

                    entityData.CreateEntity(_map);
                }
            }

        }

        public Entity FindEntity(string name)
        {
            Entity entity;
            if (!_namedEntities.TryGetValue(name, out entity))
                return null;

            if (entity.IsBeingRemoved)
                return null;

            return entity;
        }

        public Ground GetTileGround(Layer layer, int x, int y)
        {
            return _tilesGround[(int)layer, (y >> 3) * _mapWidth8 + (x >> 3)];
        }

        public IEnumerable<Entity> GetObstacleEntities(Layer layer)
        {
            return _obstacleEntities[(int)layer];
        }

        public List<Entity> GetGroundModifiers(Layer layer)
        {
            return _groundModifiers[(int)layer];
        }

        public void AddEntity(Entity entity)
        {
            if (entity == null)
                return;

            if (entity.Type != EntityType.Tile)
            {
                Layer layer = entity.Layer;

                // 디텍터 리스트를 갱신합니다
                if (entity.IsDetector)
                    _detectors.Add(entity as Detector);

                // 충돌 리스트를 갱신합니다
                if (entity.CanBeObstacle)
                {
                    if (entity.HasLayerIndependentCollisions)
                    {
                        // 계단과 같은 것들은 레이어에 상관없이 항상 충돌을 처리합니다
                        _obstacleEntities[(int)Layer.Low].Add(entity);
                        _obstacleEntities[(int)Layer.Intermediate].Add(entity);
                        _obstacleEntities[(int)Layer.High].Add(entity);
                    }
                    else
                    {
                        // 보통은 레이어 하나에서만 충돌이 필요합니다
                        _obstacleEntities[(int)layer].Add(entity);
                    }
                }

                if (entity.IsGroundModifier)
                    _groundModifiers[(int)layer].Add(entity);

                // 스프라이트 리스트를 갱신합니다
                if (entity.IsDrawnInYOrder)
                    _entitiesDrawnYOrder[(int)layer].Add(entity);
                else if (entity.CanBeDrawn)
                    _entitiesDrawnFirst[(int)layer].Add(entity);

                // 특수 타입별 엔티티 리스트를 갱신합니다
                switch (entity.Type)
                {
                    case EntityType.Destination:
                        {
                            Destination destination = entity as Destination;
                            if (DefaultDestination == null || destination.IsDefault)
                                DefaultDestination = destination;
                        }
                        break;

                    default:
                        break;
                }

                _allEntities.Add(entity);
            }

            // TODO: 같은 이름의 엔티티가 이미 있다면 이름을 바꿔줍니다
            if (entity.Name != null)
            {
                if (_namedEntities.ContainsKey(entity.Name))
                    throw new NotImplementedException("AddEntity duplicated name: {0}".F(entity.Name));

                _namedEntities.Add(entity.Name, entity);
            }

            if (entity.Type != EntityType.Hero)
                entity.SetMap(_map);
        }

        public void ScheduleAddEntity(Entity entity)
        {
            _entitiesToAdd.Add(entity);
        }
        
        internal void AddTileInfo(TileInfo tileInfo)
        {
            var box = tileInfo.Box;
            var layer = tileInfo.Layer;

            var pattern = tileInfo.Pattern;
            if (pattern == null)
                throw new InvalidOperationException("Missing tile pattern");

            if (box.Width != pattern.Width || box.Height != pattern.Height)
                throw new Exception("Static tile size must match tile pattern size");

            // 타일을 맵에 추가합니다
            _nonAnimatedRegions[(int)layer].AddTile(tileInfo);

            // Ground 리스트 갱신
            var ground = pattern.Ground;

            int tileX8 = box.X / 8;
            int tileY8 = box.Y / 8;
            int tileWidth8 = box.Width / 8;
            int tileHeight8 = box.Height / 8;

            int i = 0, j = 0;
            Ground nonObstacleTriangle = Ground.Empty;

            switch (ground)
            {
                // 8x8 모두 동일한 속성으로 채울 수 있는 타입들입니다
                case Ground.Traversable:
                case Ground.LowWall:
                case Ground.ShallowWater:
                case Ground.DeepWater:
                case Ground.Grass:
                case Ground.Hole:
                case Ground.Ice:
                case Ground.Lava:
                case Ground.Prickle:
                case Ground.Ladder:
                case Ground.Wall:
                    for (i = 0; i < tileHeight8; ++i)
                        for (j = 0; j < tileWidth8; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, ground);
                    break;

                // 타일의 상단 우측이 장애물이라면 그 부분은 Wall, 
                // 하단 좌측은 Traversable 혹은 DeepWater,
                // 나머지 부분(대각선)은 WallTopRight가 됩니다
                case Ground.WallTopRight:
                case Ground.WallTopRightWater:
                    nonObstacleTriangle = (ground == Ground.WallTopRight) ? Ground.Traversable : Ground.DeepWater;

                    // 각 행의 각 8x8 영역들을 순회합니다
                    for (i = 0; i < tileHeight8; ++i)
                    {
                        // 대각선의 8x8 영역들
                        SetTileGround(layer, tileX8 + i, tileY8 + i, Ground.WallTopRight);

                        // 행의 좌측
                        for (j = 0; j < i; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, nonObstacleTriangle);

                        // 행의 우측
                        for (j = i + 1; j < tileWidth8; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.Wall);
                    }
                    break;

                case Ground.WallTopLeft:
                case Ground.WallTopLeftWater:
                    nonObstacleTriangle = (ground == Ground.WallTopLeft) ? Ground.Traversable : Ground.DeepWater;
                    for (i = 0; i < tileHeight8; ++i)
                    {
                        for (j = tileWidth8 - i; j < tileWidth8; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, nonObstacleTriangle);

                        for (j = 0; j < tileWidth8 - i - 1; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.Wall);

                        SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.WallTopLeft);
                    }
                    break;

                case Ground.WallBottomLeft:
                case Ground.WallBottomLeftWater:
                    nonObstacleTriangle = (ground == Ground.WallBottomLeft) ? Ground.Traversable : Ground.DeepWater;
                    for (i = 0; i < tileHeight8; ++i)
                    {
                        for (j = i + 1; j < tileWidth8; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, nonObstacleTriangle);

                        for (j = 0; j < i; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.Wall);

                        SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.WallBottomLeft);
                    }
                    break;

                case Ground.WallBottomRight:
                case Ground.WallBottomRightWater:
                    nonObstacleTriangle = (ground == Ground.WallBottomRight) ? Ground.Traversable : Ground.DeepWater;
                    for (i = 0; i < tileHeight8; ++i)
                    {
                        SetTileGround(layer, tileX8 + tileWidth8 - i - 1, tileY8 + i, Ground.WallBottomRight);

                        for (j = 0; j < tileWidth8 - i - 1; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, nonObstacleTriangle);

                        for (j = tileWidth8 - i; j < tileWidth8; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.Wall);
                    }
                    break;

                case Ground.Empty:
                    // 기존 속성을 그대로 유지합니다
                    break;
            }
        }
        
        void SetTileGround(Layer layer, int x8, int y8, Ground ground)
        {
            if (x8 >= 0 && x8 < _mapWidth8 && y8 >= 0 && y8 < _mapHeight8)
            {
                int index = y8 * _mapWidth8 + x8;
                _tilesGround[(int)layer, index] = ground;
            }
        }

        public void RemoveEntity(Entity entity)
        {
            if (entity.IsBeingRemoved)
                return;

            _entitiesToRemove.Add(entity);
            entity.NotifyBeingRemoved();
        }

        void RemoveMarkedEntities()
        {
            foreach (var entity in _entitiesToRemove)
            {
                var layer = entity.Layer;

                if (entity.CanBeObstacle)
                {
                    if (entity.HasLayerIndependentCollisions)
                    {
                        for (int i = 0; i < (int)Layer.NumLayer; ++i)
                            _obstacleEntities[i].Remove(entity);
                    }
                    else
                        _obstacleEntities[(int)layer].Remove(entity);
                }

                if (entity.IsDetector)
                    _detectors.Remove(entity as Detector);

                if (entity.IsGroundModifier)
                    _groundModifiers[(int)layer].Remove(entity);

                if (entity.IsDrawnInYOrder)
                    _entitiesDrawnYOrder[(int)layer].Remove(entity);
                else if (entity.CanBeDrawn)
                    _entitiesDrawnFirst[(int)layer].Remove(entity);

                _allEntities.Remove(entity);
                if (!String.IsNullOrEmpty(entity.Name))
                    _namedEntities.Remove(entity.Name);

                NotifyEntityRemoved(entity);
            }
            _entitiesToRemove.Clear();
        }

        void NotifyEntityRemoved(Entity entity)
        {
            if (!entity.IsBeingRemoved)
                entity.NotifyBeingRemoved();
        }

        public void SetEntityDrawnInYOrder(Entity entity, bool drawnInYOrder)
        {
            int layer = (int)entity.Layer;
            if (drawnInYOrder)
            {
                _entitiesDrawnFirst[layer].Remove(entity);
                _entitiesDrawnYOrder[layer].Add(entity);
            }
            else
            {
                _entitiesDrawnYOrder[layer].Remove(entity);
                _entitiesDrawnFirst[layer].Add(entity);
            }
        }

        public void SetEntityLayer(Entity entity, Layer layer)
        {
            Layer oldLayer = entity.Layer;
            if (layer == oldLayer)
                return;

            if (entity.CanBeObstacle && !entity.HasLayerIndependentCollisions)
            {
                _obstacleEntities[(int)oldLayer].Remove(entity);
                _obstacleEntities[(int)layer].Add(entity);
            }

            if (entity.IsGroundModifier)
            {
                _groundModifiers[(int)oldLayer].Remove(entity);
                _groundModifiers[(int)layer].Add(entity);
            }

            if (entity.IsDrawnInYOrder)
            {
                _entitiesDrawnYOrder[(int)oldLayer].Remove(entity);
                _entitiesDrawnYOrder[(int)layer].Add(entity);
            }
            else if (entity.CanBeDrawn)
            {
                _entitiesDrawnFirst[(int)oldLayer].Remove(entity);
                _entitiesDrawnFirst[(int)layer].Add(entity);
            }

            entity.SetLayer(layer);
        }

        // 맵의 모든 엔티티들에게 맵이 시작(활성화)되었음을 알립니다
        public void NotifyMapStarted()
        {
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
            {
                var tilesInAnimatedRegionsInfo = new List<TileInfo>();
                _nonAnimatedRegions[layer].Build(tilesInAnimatedRegionsInfo);
                foreach (var tileInfo in tilesInAnimatedRegionsInfo)
                {
                    var tile = new Tile(tileInfo);
                    _tilesInAnimatedRegions[layer].Add(tile);
                    AddEntity(tile);
                }
            }

            foreach (var entity in _allEntities)
            {
                entity.NotifyMapStarted();
                entity.NotifyTilesetChanged();
            }
            Hero.NotifyMapStarted();
            Hero.NotifyTilesetChanged();
        }

        public void NotifyTilesetChanged()
        {
            // 최적화된 타일들을 다시그려줍니다
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _nonAnimatedRegions[layer].NotifyTilesetChanged();

            foreach (Entity entity in _allEntities)
                entity.NotifyTilesetChanged();
            Hero.NotifyTilesetChanged();
        }

        public void SetSuspended(bool suspended)
        {
            Hero.SetSuspended(suspended);

            foreach (Entity entity in _allEntities)
                entity.SetSuspended(suspended);
        }

        public void Update()
        {
            Debug.CheckAssertion(_map.IsStarted, "The map is not started");

            foreach (Entity entity in _entitiesToAdd)
                AddEntity(entity);
            _entitiesToAdd.Clear();

            Hero.Update();

            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _entitiesDrawnYOrder[layer].Sort((a, b) => (b.TopLeftY + b.Height) - (a.TopLeftY + a.Height));

            foreach (Entity entity in _allEntities)
            {
                if (!entity.IsBeingRemoved)
                    entity.Update();
            }

            RemoveMarkedEntities();
        }

        public void Draw()
        {
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
            {
                // 에니메이션되는 타일을 포함하는 영역들을 먼저 그립니다
                foreach (Tile tile in _tilesInAnimatedRegions[layer])
                    tile.DrawOnMap();

                // 애니메이션되지 않는 타일들을 그립니다
                _nonAnimatedRegions[layer].DrawOnMap();

                foreach (var entity in _entitiesDrawnFirst[layer].Where(e => e.IsEnabled))
                    entity.DrawOnMap();

                foreach (var entity in _entitiesDrawnYOrder[layer].Where(e => e.IsEnabled))
                    entity.DrawOnMap();
            }
        }
    }
}
