using System;
using System.Collections.Generic;

namespace Zelda.Game
{
    class Equipment
    {
        #region 생성
        public Equipment(Savegame saveGame)
        {
            _savegame = saveGame;
        }
        #endregion

        #region 기본
        readonly Savegame _savegame;
        public Savegame Savegame
        {
            get { return _savegame; }
        }

        public Game Game
        {
            get { return _savegame.Game; }
        }

        public void NotifyGameFinished()
        {
            foreach (EquipmentItem item in _items.Values)
                item.Exit();
        }

        public void Update()
        {
            Game game = _savegame.Game;
            if (game == null)
                return;

            bool gameSuspended = game.IsSuspended;
            if (_suspended != gameSuspended)
                SetSuspended(gameSuspended);
        }

        bool _suspended;
        public void SetSuspended(bool suspended)
        {
            _suspended = suspended;
        }
        #endregion

        #region 생명
        public int MaxLife
        {
            get
            {
                return _savegame.GetInteger(Savegame.Key.MaxLife);
            }
            set
            {
                Debug.CheckAssertion(value >= 0, "Invalid life amount");

                _savegame.SetInteger(Savegame.Key.MaxLife, value);

                if (Life > MaxLife)
                    Life = MaxLife;
            }
        }

        public int Life
        {
            get
            {
                return _savegame.GetInteger(Savegame.Key.CurrentLife);
            }
            set
            {
                int life = Math.Max(0, Math.Min(MaxLife, value));
                _savegame.SetInteger(Savegame.Key.CurrentLife, life);
            }
        }
        
        public void RestoreAllLife()
        {
            Life = MaxLife;
        }
        #endregion

        #region 장비 아이템들
        readonly Dictionary<string, EquipmentItem> _items = new Dictionary<string, EquipmentItem>();

        public void LoadItems()
        {
            // project_db.xml에 정의된 각 장비 아이템들을 생성합니다
            foreach (var kvp in CurrentMod.GetResources(ResourceType.Item))
            {
                string itemId = kvp.Key;
                EquipmentItem item = new EquipmentItem(this);
                item.Name = itemId;
                _items[itemId] = item;
            }

            // 아이템 스크립트들을 로드합니다
            foreach (EquipmentItem item in _items.Values)
                item.Initialize();

            // 아이템들을 시작합니다
            foreach (EquipmentItem item in _items.Values)
                item.Start();
        }

        public bool ItemExists(string itemName)
        {
            return _items.ContainsKey(itemName);
        }

        public EquipmentItem GetItem(string itemName)
        {
            Debug.CheckAssertion(ItemExists(itemName), "No such item: '{0}'".F(itemName));
            return _items[itemName];
        }
        #endregion

        #region 기본 제공 능력들
        public bool HasAbility(Ability ability, int level = 1)
        {
            return GetAbility(ability) >= level;
        }

        public void SetAbility(Ability ability, int level)
        {
            _savegame.SetInteger(GetAbilitySavegameVariable(ability), level);

            if (Game != null)
            {
                if (ability == Ability.Tunic ||
                    ability == Ability.Sword ||
                    ability == Ability.Shield)
                {
                    // 영웅 스프라이트가 이 능력들에 의해 영향을 받습니다
                    Game.Hero.RebuildEquipment();
                }
            }
        }

        public int GetAbility(Ability ability)
        {
            return _savegame.GetInteger(GetAbilitySavegameVariable(ability));
        }

        Savegame.Key GetAbilitySavegameVariable(Ability ability)
        {
            switch (ability)
            {
                case Ability.Tunic:
                    return Savegame.Key.AbilityTunic;

                case Ability.Sword:
                    return Savegame.Key.AbilitySword;

                case Ability.Shield:
                    return Savegame.Key.AbilityShield;

                case Ability.Lift:
                    return Savegame.Key.AbilityLift;

                case Ability.Swim:
                    return Savegame.Key.AbilitySwim;

                case Ability.Run:
                    return Savegame.Key.AbilityRun;

                case Ability.DetectWeakWalls:
                    return Savegame.Key.AbilityDetectWeakWalls;
            }

            Debug.Die("Invalid ability");
            return (Savegame.Key)(-1);
        }
        #endregion
    }
}
