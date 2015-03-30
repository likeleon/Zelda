
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

        readonly int _direction;
        readonly Layer _layer;
        readonly Rectangle _boundingBox;
        MainLoop _mainLoop;
        bool _initialized;

        protected MapEntity(string name, int direction, Layer layer, Point xy, Size size)
        {
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
