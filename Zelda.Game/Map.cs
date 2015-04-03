using System;
using Zelda.Game.Engine;
using Zelda.Game.Entities;
using ScriptContext = Zelda.Game.Script.ScriptContext;

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

        public bool IsStarted
        {
            get { return false; }
        }

        Rectangle _location;
        public Rectangle Location
        {
            get { return _location; }
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

        MapEntities _entities;
        public MapEntities Entities
        {
            get { return _entities; }
        }

        Game _game;
        public Game Game
        {
            get { return _game; }
        }

        Surface _visibleSurface;
        public Surface VisibleSurface
        {
            get { return _visibleSurface; }
        }

        Surface _backgroundSurface;
        Surface _foregroundSurface;

        public Map(string id)
        {
            _id = id;
        }

        public void Load(Game game)
        {
            _visibleSurface = Surface.Create(Video.ModSize);
            _visibleSurface.SoftwareDestination = false;

            _backgroundSurface = Surface.Create(Video.ModSize);
            _backgroundSurface.SoftwareDestination = false;

            _entities = new MapEntities(game, this);

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
            _world = data.World;
            _floor = data.Floor;
            _tilesetId = data.TilesetId;
            _tileset = new Tileset(data.TilesetId);
            _tileset.Load();

            // 엔티티들을 생성합니다
            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
            {
                for (int i = 0; i < data.GetNumEntities(layer); ++i)
                {
                    EntityData entityData = data.GetEntity(new EntityIndex(layer, i));
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

        void DrawBackground()
        {
            _backgroundSurface.Draw(_visibleSurface);
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
                dstPosition = new Rectangle(barWidth + Width, 0, barWidth, screenHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
            }

            if (Height < screenHeight)
            {
                int barHeight = (screenHeight - Height) / 2;
                Rectangle dstPosition = new Rectangle(0, 0, screenWidth, barHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
                dstPosition = new Rectangle(0, barHeight + Height, screenWidth, barHeight);
                _foregroundSurface.FillWithColor(Color.Black, dstPosition);
            }
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
                //_entities.Draw();
                DrawForeground();
            }
        }
    }
}
