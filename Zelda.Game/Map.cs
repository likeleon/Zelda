using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Map
    {
        readonly string _id;
        public string Id
        {
            get { return _id; }
        }

        string _destinationName;
        public string DestinationName
        {
            get { return _destinationName; }
            set { _destinationName = value; }
        }

        bool _loaded;
        public bool IsLoaded
        {
            get { return _loaded; }
        }

        Rectangle _location;
        public Rectangle Location
        {
            get { return _location; }
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

        Game _game;

        public Map(string id)
        {
            _id = id;
        }

        public void Load(Game game)
        {
            // 맵 데이터 파일을 읽습니다.
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
            _world = data.World;
            _floor = data.Floor;

            _loaded = true;
        }
    }
}
