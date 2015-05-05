using System;

namespace Zelda.Game
{
    class EquipmentItem
    {
        public EquipmentItem(Equipment equipment)
        {
            _equipment = equipment;
            Name = String.Empty;
            SaveGameVariable = String.Empty;
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
            get { return !String.IsNullOrEmpty(SaveGameVariable); }
        }

        public string SaveGameVariable { get; set; }
        #endregion
    }
}
