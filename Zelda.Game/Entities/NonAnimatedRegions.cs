using System.Collections.Generic;
using Zelda.Game.Containers;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    // 애니메이션되지 않는 영역에 존재하는 타일들을 관리합니다
    class NonAnimatedRegions
    {
        readonly Map _map;
        readonly Layer _layer;
        readonly List<Tile> _tiles = new List<Tile>();
        readonly bool[] _areSquaresAnimated;
        readonly Grid<Tile> _nonAnimatedTiles;
        Surface[] _optimizedTilesSurfaces;

        public NonAnimatedRegions(Map map, Layer layer)
        {
            _map = map;
            _layer = layer;
            _nonAnimatedTiles = new Grid<Tile>(map.Size, new Size(512, 256));
            _areSquaresAnimated = new bool[_map.Width8 * _map.Height8];
        }

        public void AddTile(Tile tile)
        {
            Debug.CheckAssertion(_optimizedTilesSurfaces == null, "Tile regions are already built");
            Debug.CheckAssertion(tile != null, "Missing tile");
            Debug.CheckAssertion(tile.Layer == _layer, "Wrong layer for add tile");

            _tiles.Add(tile);
        }

        // 애니메이션되지 않는 타일들을 한번만 그리기 위해 애니메이션되는 것들을 찾아냅니다
        public void Build(List<Tile> rejectedTiles)
        {
            Debug.CheckAssertion(_optimizedTilesSurfaces == null, "Tile regions are already built");

            for (int i = 0; i < _areSquaresAnimated.Length; ++i)
                _areSquaresAnimated[i] = false;
        
            _optimizedTilesSurfaces = new Surface[_nonAnimatedTiles.NumCells];

            // 맵 상에서 애니메이션되어야 하는 8x8 영역들을 표시해둡니다
            foreach (Tile tile in _tiles)
            {
                if (!tile.IsAnimated)
                    continue;

                int tileX8 = tile.X / 8;
                int tileY8 = tile.Y / 8;
                int tileWidth8 = tile.Width / 8;
                int tileHeight8 = tile.Height / 8;

                for (int y = 0; y < tileHeight8; ++y)
                {
                    for (int x = 0; x < tileWidth8; ++x)
                    {
                        int x8 = tileX8 + x;
                        int y8 = tileY8 + y;
                        if (x8 >= 0 && x8 < _map.Width8 && y8 >= 0 && y8 < _map.Height8)
                        {
                            int index = y8 * _map.Width8 + x8;
                            _areSquaresAnimated[index] = true;
                        }
                    }
                }
            }

            // 애니메이션되는 타일들의 리스트를 구성합니다
            foreach (Tile tile in _tiles)
            {
                if (!tile.IsAnimated)
                {
                    _nonAnimatedTiles.Add(tile, tile.BoundingBox);
                    if (OverlapsAnimatedTile(tile))
                        rejectedTiles.Add(tile);
                }
                else
                {
                    rejectedTiles.Add(tile);
                }
            }

            // 이제 전체 타일을 유지할 필요는 없습니다.
            // 애니메이션되지 않는 것들만 유지합니다.
            _tiles.Clear();
        }

        // 'tile'이 애니메이션되는 다른 타일과 겹치는지를 반환합니다 
        bool OverlapsAnimatedTile(Tile tile)
        {
            if (tile.Layer != _layer)
                return false;

            int tileX8 = tile.X / 8;
            int tileY8 = tile.Y / 8;
            int tileWidth8 = tile.Width / 8;
            int tileHeight8 = tile.Height / 8;

            for (int y = 0; y < tileHeight8; ++y)
            {
                for (int x = 0; x < tileWidth8; ++x)
                {
                    int x8 = tileX8 + x;
                    int y8 = tileY8 + y;
                    if (x8 >= 0 && x8 < _map.Width8 && y8 >= 0 && y8 < _map.Height8)
                    {
                        int index = y8 * _map.Width8 + x8;
                        if (_areSquaresAnimated[index])
                            return true;
                    }
                }
            }
            return false;
        }

        public void NotifyTilesetChanged()
        {
            for (int i = 0; i < _nonAnimatedTiles.NumCells; ++i)
                _optimizedTilesSurfaces[i] = null;
            // 필요한 시점에 다시 그려집니다
        }

        public void DrawOnMap()
        {
            // 카메라와 겹치는 그리드 셀들을 체크합니다
            int numRows = _nonAnimatedTiles.NumRows;
            int numColumns = _nonAnimatedTiles.NumColumns;
            Size cellSize = _nonAnimatedTiles.CellSize;
            Rectangle cameraPosition = _map.CameraPosition;

            int row1 = cameraPosition.Y / cellSize.Height;
            int row2 = (cameraPosition.Y + cameraPosition.Height) / cellSize.Height;
            int column1 = cameraPosition.X / cellSize.Width;
            int column2 = (cameraPosition.X + cameraPosition.Width) / cellSize.Width;

            if (row1 > row2 || column1 > column2)
                return; // no cell

            for (int y = row1; y <= row2; ++y)
            {
                if (y < 0 || y >= numRows)
                    continue;

                for (int x = column1; x <= column2; ++x)
                {
                    if (x < 0 || x >= numColumns)
                        continue;

                    // 이 셀이 제대로 구성되었음을 보장해야 합니다
                    int cellIndex = y * numColumns + x;
                    if (_optimizedTilesSurfaces[cellIndex] == null)
                        BuildCell(cellIndex);

                    Point cellXY = new Point(x * cellSize.Width, y * cellSize.Height);
                    Point dstPosition = cellXY - cameraPosition.XY;
                    _optimizedTilesSurfaces[cellIndex].Draw(_map.VisibleSurface, dstPosition);
                }
            }
        }

        // 셀의 모든 애니메이션되지 않는 타일들을 표면에 그려둡니다
        void BuildCell(int cellIndex)
        {
            Debug.CheckAssertion(cellIndex >= 0 && cellIndex < _nonAnimatedTiles.NumCells, "Wrong cell index");
            Debug.CheckAssertion(_optimizedTilesSurfaces[cellIndex] == null, "This cell is already built");

            int row = cellIndex / _nonAnimatedTiles.NumColumns;
            int column = cellIndex % _nonAnimatedTiles.NumColumns;

            // 맵상에서 이 셀의 위치
            Size cellSize = _nonAnimatedTiles.CellSize;
            Point cellXY = new Point(column * cellSize.Width, row * cellSize.Height);

            Surface cellSurface = Surface.Create(cellSize);
            _optimizedTilesSurfaces[cellIndex] = cellSurface;

            foreach (Tile tile in _nonAnimatedTiles.GetElements(cellIndex))
                tile.Draw(cellSurface, cellXY);

            // 애니메이션되지 않는 타일들은 애니메이션되는 타일들 다음에 그려지기 때문에,
            // 애니메이션이 필요한 영역에는 그려져서는 안 됩니다.
            // 애니메이션 타일이 위치하는 8x8 영역들을 지워줍니다
            for (int y = cellXY.Y; y < cellXY.Y + cellSize.Height; y += 8)
            {
                if (y >= _map.Height)
                    continue;   // 마지막 셀은 맵 범위를 벗어날 수 있습니다

                for (int x = cellXY.X; x < cellXY.X + cellSize.Width; x += 8)
                {
                    if (x >= _map.Width)
                        continue;

                    int squareIndex = (y / 8) * _map.Width8 + (x / 8);
                    if (_areSquaresAnimated[squareIndex])
                    {
                        Rectangle animatedSqure = new Rectangle(x - cellXY.X, y - cellXY.Y, 8, 8);
                        cellSurface.Clear(animatedSqure);
                    }
                }
            }
        }
    }
}
