using System;
using System.Collections.Generic;

namespace Zelda.Game.Entities
{
    // 맵 컨텐츠를 관리합니다 (타일, 주인공, 적과 같은 맵 엔티티들을 저장하고 관리)
    class MapEntities
    {
        readonly Game _game;
        readonly Map _map;
        readonly int _mapWidth8;
        readonly int _mapHeight8;
        readonly int _tilesGridSize;
        readonly Ground[,] _tilesGround;
        readonly Dictionary<string, MapEntity> _namedEntities = new Dictionary<string, MapEntity>();
        readonly NonAnimatedRegions[] _nonAnimatedRegions = new NonAnimatedRegions[(int)Layer.Count];
        readonly List<MapEntity> _allEntities = new List<MapEntity>();

        public MapEntities(Game game, Map map)
        {
            _game = game;
            _map = map;
            _mapWidth8 = _map.Width8;
            _mapHeight8 = _map.Height8;
            _tilesGridSize = _mapWidth8 * _mapHeight8;

            _tilesGround = new Ground[(int)Layer.Count, _tilesGridSize];
            for (int layer = 0; layer < (int)Layer.Count; ++layer)
            {
                Ground initialGround = ((Layer)layer == Layer.Low) ? Ground.Traversable : Ground.Empty;
                for (int i = 0; i < _tilesGridSize; ++i)
                    _tilesGround[layer, i] = initialGround;

                _nonAnimatedRegions[layer] = new NonAnimatedRegions(_map, (Layer)layer);
            }
        }

        public void AddEntity(MapEntity entity)
        {
            if (entity == null)
                return;

            if (entity.Type == EntityType.Tile)
            {
                // 타일들은 충돌 체크와 렌더링에 최적화가 필요합니다
                AddTile(entity as Tile);
            }
            else
            {
                _allEntities.Add(entity);
            }

            // TODO: 같은 이름의 엔티티가 이미 있다면 이름을 바꿔줍니다
            if (!String.IsNullOrEmpty(entity.Name))
            {
                if (_namedEntities.ContainsKey(entity.Name))
                {
                    throw new NotImplementedException("AddEntity duplicated name: {0}".F(entity.Name));
                }
            }

            entity.SetMap(_map);
        }

        void AddTile(Tile tile)
        {
            Layer layer = tile.Layer;

            // 타일을 맵에 추가합니다
            _nonAnimatedRegions[(int)layer].AddTile(tile);

            TilePattern pattern = tile.Pattern;
            Debug.CheckAssertion(
                tile.Width == pattern.Width &&
                tile.Height == pattern.Height,
                "Static tile size must match tile pattern size");

            // Ground 리스트 갱신
            Ground ground = pattern.Ground;

            int tileX8 = tile.X / 8;
            int tileY8 = tile.Y / 8;
            int tileWidth8 = tile.Width / 8;
            int tileHeight8 = tile.Height / 8;

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
                    nonObstacleTriangle = (ground == Ground.WallTopRight) ?
                        Ground.Traversable : Ground.DeepWater;

                    // 각 행의 각 8x8 영역들을 순회합니다
                    for (i = 0; i < tileHeight8; ++i)
                    {
                        // 대각선의 8x8 영역들
                        SetTileGround(layer, tileX8 + i, tileY8 + i, Ground.WallTopRight);

                        // 행의 좌측
                        for (j = 0; j < i; ++j)
                            SetTileGround(layer, tileX8, tileY8 + i, nonObstacleTriangle);

                        // 행의 우측
                        for (j = i + 1; j < tileWidth8; ++j)
                            SetTileGround(layer, tileX8 + j, tileY8 + i, Ground.Wall);
                    }
                    break;

                case Ground.WallTopLeft:
                case Ground.WallTopLeftWater:
                    nonObstacleTriangle = (ground == Ground.WallTopLeft) ?
                        Ground.Traversable : Ground.DeepWater;
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
                    nonObstacleTriangle = (ground == Ground.WallBottomLeft) ?
                        Ground.Traversable : Ground.DeepWater;
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
                    nonObstacleTriangle = (ground == Ground.WallBottomRight) ?
                        Ground.Traversable : Ground.DeepWater;
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
    }
}
