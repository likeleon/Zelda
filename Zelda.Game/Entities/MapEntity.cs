
using Zelda.Game.Engine;
namespace Zelda.Game.Entities
{
    abstract class MapEntity
    {
        public abstract EntityType Type { get; }

        public string Name { get; set; }

        Map _map;
        public Map Map
        {
            get { return _map; }
        }

        public bool IsOnMap
        {
            get { return _map != null; }
        }

        public Game Game
        {
            get
            {
                Debug.CheckAssertion(Map != null, "No map was set");
                return _map.Game;
            }
        }

        Ground _groundBelow;
        public Ground GroundBelow
        {
            get { return _groundBelow; }
        }

        readonly Layer _layer;
        public Layer Layer
        {
            get { return _layer; }
        }

        Rectangle _boundingBox;
        public int Width
        {
            get { return _boundingBox.Width; }
        }

        public int Height
        {
            get { return _boundingBox.Height; }
        }

        Point _origin;
        public Point Origin
        {
            get { return _origin; }
            set 
            {
                _boundingBox = new Rectangle(_boundingBox.XY + (_origin - value), _boundingBox.Size);
                _origin = value; 
            }
        }

        public int X
        {
            get { return _boundingBox.X + _origin.X; }
        }

        public int Y
        {
            get { return _boundingBox.Y + _origin.Y; }
        }

        public Point XY
        {
            get { return new Point(X, Y); }
        }

        readonly int _direction;
        MainLoop _mainLoop;
        bool _initialized;

        protected MapEntity(string name, int direction, Layer layer, Point xy, Size size)
        {
            Debug.CheckAssertion(size.Width % 8 == 0 && size.Height % 8 == 0,
                "Invalid entity size: width and height must be multiple of 8");

            Name = name;
            _direction = direction;
            _layer = layer;
            _boundingBox = new Rectangle(xy, size);
        }

        public void SetMap(Map map)
        {
            _mainLoop = map.Game.MainLoop;
            _map = map;

            _groundBelow = Ground.Empty;

            if (!_initialized && _map.IsLoaded)
            {
                // 엔티티가 이미 실행중인 맵에 생성된 경우로, 초기화를 지금 바료 완료할 수 있습니다
                FinishInitialization();
            }
        }

        private void FinishInitialization()
        {
            Debug.CheckAssertion(!_initialized, "Entity is already initialized");
            Debug.CheckAssertion(IsOnMap, "Missing map");
            Debug.CheckAssertion(Map.IsLoaded, "Map is not ready");

            _initialized = true;

            NotifyCreating();
            //ScriptContext.EntityOnCreated(this);
            NotifyCreated();
        }

        public virtual void NotifyCreating()
        {
        }

        public virtual void NotifyCreated()
        {
        }
    }
}
