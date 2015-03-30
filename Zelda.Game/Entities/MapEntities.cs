using System;
using System.Collections.Generic;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    // 맵 컨텐츠를 관리합니다 (타일, 주인공, 적과 같은 맵 엔티티들을 저장하고 관리)
    class MapEntities
    {
        readonly Game _game;
        readonly Map _map;
        int _mapWidth8;
        int _mapHeight8;
        int _tilesGridSize;
        readonly List<Ground>[] _tilesGround = new List<Ground>[Enum.GetValues(typeof(Layer)).Length];
        readonly Dictionary<string, MapEntity> _namedEntities = new Dictionary<string, MapEntity>();

        public MapEntities(Game game, Map map)
        {
            _game = game;
            _map = map;

            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
                _tilesGround[(int)layer] = new List<Ground>();
        }

        public void Initialize(MapData mapData)
        {
            _mapWidth8 = _map.Width8;
            _mapHeight8 = _map.Height8;
            _tilesGridSize = _mapWidth8 * _mapHeight8;
    
            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
            {
                Ground initialGround = (layer == Layer.Low) ? Ground.Traversable : Ground.Empty;
                for (int i = 0; i < _tilesGridSize; ++i)
                    _tilesGround[(int)layer].Add(initialGround);
            }

            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
            {
                for (int i = 0; i < mapData.GetNumEntities(layer); ++i)
                {
                    EntityData entityData = mapData.GetEntity(new EntityIndex(layer, i));
                    EntityType type = entityData.Type;
                    if (!type.CanBeStoredInMapFile())
                        Debug.Error("Illegal entity type in map file: " + type);
                    
                    ScriptContext.CreateMapEntityFromData(_map, entityData);
                }
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
                throw new NotImplementedException("AddEntity with type: {0}".F(entity.Type));
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
            throw new NotImplementedException("AddTile");
        }
    }
}
