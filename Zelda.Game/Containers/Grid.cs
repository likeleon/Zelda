using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Containers
{
    // 그리드 위에 위치하는 오브젝트들의 컬렉션
    class Grid<T>
    {
        readonly Size _gridSize;
        public Size GridSize
        {
            get { return _gridSize; }
        }

        readonly Size _cellSize;
        public Size CellSize
        {
            get { return _cellSize; }
        }

        readonly int _numRows;
        public int NumRows
        {
            get { return _numRows; }
        }

        readonly int _numColumns;
        public int NumColumns
        {
            get { return _numColumns; }
        }

        readonly List<T>[] _elements;
        public int NumCells
        {
            get { return _elements.Length; }
        }

        public Grid(Size gridSize, Size cellSize)
        {
            _gridSize = gridSize;
            _cellSize = cellSize;

            Debug.CheckAssertion(_gridSize.Width > 0 && _gridSize.Height > 0, "Invalid grid size");
            Debug.CheckAssertion(_cellSize.Width > 0 && _cellSize.Height > 0, "Invalid cell size");

            _numRows = _gridSize.Height / _cellSize.Height;
            if (_gridSize.Height % _cellSize.Height != 0)
                ++_numRows;

            _numColumns = _gridSize.Width / _cellSize.Width;
            if (_gridSize.Width % _cellSize.Width != 0)
                ++_numColumns;

            _elements = new List<T>[_numRows * _numColumns];
            for (int i = 0; i < _elements.Length; ++i)
                _elements[i] = new List<T>();
        }

        public void Clear()
        {
            for (int i = 0; i < _elements.Length; ++i)
                _elements[i] = new List<T>();
        }

        public void Add(T element, Rectangle position)
        {
            int row1 = position.Y / _cellSize.Height;
            int row2 = (position.Y + position.Height) / _cellSize.Height;
            int column1 = position.X / _cellSize.Width;
            int column2 = (position.X + position.Width) / _cellSize.Width;

            if (row1 > row2 || column1 > column2)
                return; // no cell

            for (int i = row1; i <= row2; ++i)
            {
                if (i >= _numRows)
                    continue;

                for (int j = column1; j <= column2; ++j)
                {
                    if (j >= _numColumns)
                        continue;

                    _elements[i * _numColumns + j].Add(element);
                }
            }
        }

        public IEnumerable<T> GetElements(int cellIndex)
        {
            Debug.CheckAssertion(cellIndex < NumCells, "Invalid index");
            return _elements[cellIndex];
        }

        public void GetElements(Rectangle where, List<T> elements)
        {
            int row1 = where.Y / _numRows;
            int row2 = where.Y + where.Height / _numRows;
            int column1 = where.X / _numColumns;
            int column2 = where.X + where.Width / _numColumns;

            if (row1 > row2 || column1 > column2)
                return; // no cell

            HashSet<T> elementsAdded = new HashSet<T>();
            for (int i = row1; i <= row2; ++i)
            {
                if (i >= _numRows)
                    continue;
                
                for (int j = column1; j <= column2; ++j)
                {
                    if (j >= _numColumns)
                        continue;

                    List<T> inCell = _elements[i * _numColumns + j];
                    foreach (T element in inCell)
                    {
                        if (!elementsAdded.Contains(element))
                        {
                            elementsAdded.Add(element);
                            elements.Add(element);
                        }
                    }
                }
            }
        }
    }
}
