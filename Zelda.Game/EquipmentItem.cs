﻿using System;

namespace Zelda.Game
{
    class EquipmentItem
    {
        public EquipmentItem(Equipment equipment)
        {
            _equipment = equipment;
            Name = String.Empty;
            SavegameVariable = String.Empty;
            IsObtainable = true;
            SoundWhenBrandished = "treasure";
        }

        readonly Equipment _equipment;
        public Equipment Equipment
        {
            get { return _equipment; }
        }

        public Game Game
        {
            get { return _equipment.Game; }
        }

        public Savegame Savegame
        {
            get { return _equipment.Savegame; }
        }

        #region 속성들
        public string Name { get; set; }

        public bool IsSaved
        {
            get { return !String.IsNullOrEmpty(SavegameVariable); }
        }

        public string SavegameVariable { get; set; }
        public string AmountSavegameVariable { get; set; }
        public bool HasAmount
        {
            get { return !String.IsNullOrEmpty(AmountSavegameVariable); }
        }

        public bool IsObtainable { get; set; }

        public string SoundWhenBrandished { get; set; }
        #endregion

        #region 현재 상태
        public int Variant
        {
            get
            {
                Debug.CheckAssertion(IsSaved, "The item '{0}' is not saved".F(Name));
                return Savegame.GetInteger(SavegameVariable);
            }
        }

        public void SetVariant(int variant)
        {
            Debug.CheckAssertion(IsSaved, "The item '{0}' is not saved".F(Name));

            Savegame.SetInteger(SavegameVariable, variant);
        }

        public int Amount
        {
            get
            {
                Debug.CheckAssertion(HasAmount, "The item '{0}' has no amount".F(Name));
                return Savegame.GetInteger(AmountSavegameVariable);
            }
        }
        #endregion
    }
}
