using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    abstract class State
    {
        bool _stopping;
        protected bool IsStopping
        {
            get { return _stopping; }
        }

        protected State(Hero hero, string stateName)
        {
            _hero = hero;
            _name = stateName;
        }

        #region 생성과 소멸
        readonly string _name;
        public string Name
        {
            get { return _name; }
        }

        public virtual void Start(State previousState)
        {
            SetSuspended(Hero.IsSuspended);
        }

        public virtual void Stop(State nextState)
        {
            Debug.CheckAssertion(!IsStopping, "This state is already stopping: " + Name);
            _stopping = true;
        }
        #endregion

        #region 게임 루프
        public virtual void Update()
        {
        }

        public virtual void DrawOnMap()
        {
            Sprites.DrawOnMap();
        }

        uint _whenSuspended;
        public uint WhenSuspended
        {
            get { return _whenSuspended; }
        }

        bool _suspended;
        public virtual void SetSuspended(bool suspended)
        {
            if (suspended == _suspended)
                return;

            _suspended = suspended;

            if (suspended)
                _whenSuspended = EngineSystem.Now;
        }
        #endregion

        #region 게임 오브젝트 접근
        readonly Hero _hero;
        protected Hero Hero
        {
            get { return _hero; }
        }

        protected HeroSprites Sprites
        {
            get { return _hero.HeroSprites; }
        }

        Map _map;
        protected Map Map
        {
            get { return _map; }
        }

        protected Game Game
        {
            get { return _map.Game; }
        }

        protected CommandsEffects CommandsEffects
        {
            get { return Game.CommandsEffects; }
        }
        #endregion

        #region 게임
        public virtual void SetMap(Map map)
        {
            _map = map;
        }
        #endregion

        #region 스프라이트
        public virtual bool IsHeroVisible
        {
            get { return true; }
        }
        #endregion

        #region 이동
        public virtual int WantedMovementDirection8
        {
            get { return -1; }
        }

        public virtual void NotifyMovementChanged()
        {
        }

        public virtual void NotifyPositionChanged()
        {
        }
        #endregion

        protected bool IsCurrentState
        {
            get { return (_hero.State == this) && (!_hero.State.IsStopping); }
        }
    }
}
