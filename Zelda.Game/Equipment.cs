﻿using System;
using System.Collections.Generic;

namespace Zelda.Game
{
    public class Equipment
    {
        readonly Dictionary<string, EquipmentItem> _items = new Dictionary<string, EquipmentItem>();
        bool _suspended;

        public Savegame Savegame { get; }
        public Game Game { get { return Savegame.Game; } }
        
        public Equipment(Savegame saveGame)
        {
            Savegame = saveGame;
        }

        public void NotifyGameFinished()
        {
            _items.Values.Do(i => i.Finish());
        }

        public void Update()
        {
            if (Game == null)
                return;

            if (_suspended != Game.IsSuspended)
                SetSuspended(Game.IsSuspended);
        }

        public void SetSuspended(bool suspended)
        {
            _suspended = suspended;
        }

        public int GetMaxLife()
        {
            return Savegame.GetInteger(Savegame.Key.MaxLife);
        }
        
        public void SetMaxLife(int maxLife)
        {
            Debug.CheckAssertion(maxLife >= 0, "Invalid life amount");

            Savegame.SetInteger(Savegame.Key.MaxLife, maxLife);

            if (GetLife() > GetMaxLife())
                SetLife(maxLife);
        }

        public int GetLife()
        {
            return Savegame.GetInteger(Savegame.Key.CurrentLife);
        }

        public void SetLife(int life)
        {
            life = Math.Max(0, Math.Min(GetMaxLife(), life));
            Savegame.SetInteger(Savegame.Key.CurrentLife, life);
        }

        public void RemoveLife(int lifeToRemove)
        {
            Debug.CheckAssertion(lifeToRemove >= 0, "Invalid life amount to remove");
            SetLife(GetLife() - lifeToRemove);
        }
        
        public void RestoreAllLife()
        {
            SetLife(GetMaxLife());
        }

        public void LoadItems()
        {
            // project_db.xml에 정의된 각 장비 아이템들을 생성합니다
            Core.Mod.GetResources(ResourceType.Item).Keys.Do(k => _items.Add(k, CreateItem(k)));
            _items.Values.Do(i => i.Start());
        }

        EquipmentItem CreateItem(string itemId)
        {
            var type = Core.Mod.ObjectCreator.GetTypeById<EquipmentItem>(itemId);
            if (type == null)
                return new EquipmentItem(this, itemId);

            var args = new Dictionary<string, object>() { { "equipment", this }, { "name", itemId } };
            return Core.Mod.ObjectCreator.CreateObject<EquipmentItem>(type.Name, args);
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

        public EquipmentItem GetItemAssigned(int slot)
        {
            Debug.CheckAssertion(slot >= 1 && slot <= 2, "Invalid item slot");
            var savegameVariable = "_item_slot_" + slot;
            var itemName = Savegame.GetString(savegameVariable);

            if (itemName.IsNullOrEmpty())
                return null;
            
            return GetItem(itemName);
        }

        public void SetItemAssigned(int slot, EquipmentItem item)
        {
            Debug.CheckAssertion(slot >= 1 && slot <= 2, "Invalid item slot");

            var key = "_item_slot_" + slot;

            if (item != null)
            {
                Debug.CheckAssertion(item.Variant > 0,
                    "Cannot assign item '{0}' because the player does not have it".F(item.Name));
                Debug.CheckAssertion(item.IsAssignable,
                    "The item '{0}' cannot be assigned".F(item.Name));
                Savegame.SetString(key, item.Name);
            }
            else
                Savegame.SetString(key, String.Empty);
        }

        public bool HasAbility(Ability ability, int level = 1)
        {
            return GetAbility(ability) >= level;
        }

        public void SetAbility(Ability ability, int level)
        {
            Savegame.SetInteger(GetAbilitySavegameVariable(ability), level);

            if (Game != null)
            {
                if (ability == Ability.Tunic || ability == Ability.Sword || ability == Ability.Shield)
                {
                    // 영웅 스프라이트가 이 능력들에 의해 영향을 받습니다
                    Game.Hero.RebuildEquipment();
                }
            }
        }

        public int GetAbility(Ability ability)
        {
            return Savegame.GetInteger(GetAbilitySavegameVariable(ability));
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

        public void NotifyAbilityUsed(Ability ability)
        {
            _items.Values.Do(i => i.NotifyAbilityUsed(ability));
        }
    }
}
