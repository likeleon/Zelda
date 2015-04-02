using System.Collections.Generic;

namespace Zelda.Game.Entities
{
    // 애니메이션되지 않는 영역에 존재하는 타일들을 관리합니다
    class NonAnimatedRegions
    {
        readonly Map _map;
        readonly Layer _layer;
        readonly List<Tile> _tiles = new List<Tile>();

        public NonAnimatedRegions(Map map, Layer layer)
        {
            _map = map;
            _layer = layer;
        }

        public void AddTile(Tile tile)
        {
            Debug.CheckAssertion(tile != null, "Missing tile");
            Debug.CheckAssertion(tile.Layer == _layer, "Wrong layer for add tile");

            _tiles.Add(tile);
        }
    }
}
