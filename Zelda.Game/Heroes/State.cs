using System;
using Zelda.Game.Lowlevel;
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
            Map = hero.Map;
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
        public bool IsSuspended
        {
            get { return _suspended; }
        }

        public virtual void SetSuspended(bool suspended)
        {
            if (suspended == _suspended)
                return;

            _suspended = suspended;

            if (suspended)
                _whenSuspended = Engine.Now;
        }

        public void NotifyCommandPressed(GameCommand command)
        {
            switch (command)
            {
                case GameCommand.Action:
                    NotifyActionCommandPressed();
                    break;

                case GameCommand.Item1:
                    NotifyItemCommandPressed(1);
                    break;

                case GameCommand.Item2:
                    NotifyItemCommandPressed(2);
                    break;

                default:
                    break;
            }
        }

        public virtual void NotifyActionCommandPressed()
        {
        }

        public virtual void NotifyItemCommandPressed(int slot)
        {
            EquipmentItem item = Equipment.GetItemAssigned(slot);
            if (item != null && Hero.CanStartItem(item))
                Hero.StartItem(item);
        }
        #endregion

        #region 게임 오브젝트 접근
        protected MapEntities Entities
        {
            get { return Map.Entities; }
        }

        readonly Hero _hero;
        protected Hero Hero
        {
            get { return _hero; }
        }

        protected HeroSprites Sprites
        {
            get { return _hero.HeroSprites; }
        }

        protected Map Map { get; private set; }

        protected Equipment Equipment 
        { 
            get { return Game.Equipment; } 
        }

        protected Game Game
        {
            get { return Map.Game; }
        }

        protected CommandsEffects CommandsEffects
        {
            get { return Game.CommandsEffects; }
        }

        public GameCommands Commands
        {
            get { return Game.Commands; }
        }
        #endregion

        #region 게임
        public virtual void SetMap(Map map)
        {
            Map = map;
        }
        #endregion

        #region 스프라이트
        public virtual bool IsHeroVisible
        {
            get { return true; }
        }
        #endregion

        #region 이동
        public virtual Direction8 WantedMovementDirection8
        {
            get { return Direction8.None; }
        }

        public virtual void NotifyMovementChanged()
        {
        }

        public virtual void NotifyMovementFinished()
        {
        }

        public virtual void NotifyPositionChanged()
        {
        }

        public virtual void NotifyLayerChanged()
        {
        }
        #endregion

        protected bool IsCurrentState
        {
            get { return (_hero.State == this) && (!_hero.State.IsStopping); }
        }

        #region 적들
        public virtual bool CanBeHurt(MapEntity attacker)
        {
            return false;
        }
        #endregion

        #region 상태별
        public virtual bool IsFree
        {
            get { return false; }
        }

        public virtual bool IsUsingItem
        {
            get { return false; }
        }

        public virtual EquipmentItemUsage ItemBeingUsed
        {
            get
            {
                Debug.Die("No item is being used in this state");
                throw new Exception();
            }
        }

        public virtual bool IsBrandishingTreasure
        {
            get { return false; }
        }

        public virtual bool IsGrabbingOrPulling
        {
            get { return false; }
        }

        public virtual bool IsMovingGrabbedEntity
        {
            get { return false; }
        }

        public virtual bool CanStartItem(EquipmentItem item)
        {
            return false;
        }

        public virtual void NotifyGrabbedEntityCollision()
        {
        }

        public bool IsCarryingItem
        {
            get { return CarriedItem != null; }
        }

        public virtual CarriedItem CarriedItem
        {
            get { return null; }
        }

        public virtual CarriedItem.Behavior PreviousCarriedItemBehavior
        {
            get { return CarriedItem.Behavior.Throw; }
        }
        #endregion

        #region 장애물 & 충돌
        public virtual bool AreCollisionsIgnored
        {
            get { return false; }
        }

        public virtual bool IsShallowWaterObstacle
        {
            get { return false; }
        }

        public virtual bool IsDeepWaterObstacle
        {
            get { return false; }
        }

        public virtual bool IsHoleObstacle
        {
            get { return false; }
        }

        public virtual bool IsLavaObstacle
        {
            get { return false; }
        }

        public virtual bool IsPrickleObstacle
        {
            get { return false; }
        }

        public virtual bool IsLadderObstacle
        {
            get { return false; }
        }

        public virtual bool CanAvoidExplosion
        {
            get { return false; }
        }
        #endregion
    }
}
