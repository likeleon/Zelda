using System.Collections.Generic;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    // 애니메이션되지 않는 영역에 존재하는 타일들을 관리합니다
    class NonAnimatedRegions
    {
        readonly Map _map;
        readonly Layer _layer;
        readonly List<Tile> _tiles = new List<Tile>();
        readonly bool[] _areSquaresAnimated;
        readonly Tile
        readonly List<Surface> _optimizedTilesSurface = new List<Surface>();

        public NonAnimatedRegions(Map map, Layer layer)
        {
            _map = map;
            _layer = layer;

            _areSquaresAnimated = new bool[_map.Width8 * _map.Height8];
        }

        public void AddTile(Tile tile)
        {
            Debug.CheckAssertion(_optimizedTilesSurface.Count <= 0, "Tile regions are already built");
            Debug.CheckAssertion(tile != null, "Missing tile");
            Debug.CheckAssertion(tile.Layer == _layer, "Wrong layer for add tile");

            _tiles.Add(tile);
        }

        // 애니메이션되지 않는 타일들을 한번만 그리기 위해 애니메이션되는 것들을 찾아냅니다
        public void Build(List<Tile> rejectedTiles)
        {
            Debug.CheckAssertion(_optimizedTilesSurface.Count <= 0, "Tile regions are already built");

            for (int i = 0; i < _optimizedTilesSurface.Count; ++i)
                _areSquaresAnimated[i] = false;
        }
    }
}
