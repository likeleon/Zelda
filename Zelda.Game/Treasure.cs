using System;

namespace Zelda.Game
{
    class Treasure
    {
        readonly Game _game;
        string _itemName;
        int _variant;

        public string SavegameVariable { get; private set; }

        public Treasure(Game game, string itemName, int variant, string savegameVariable)
        {
            _game = game;
            _itemName = itemName;
            _variant = variant;
            SavegameVariable = savegameVariable;
        }

        public bool IsSaved
        {
            get { return !String.IsNullOrEmpty(SavegameVariable); }
        }

        public bool IsFound
        {
            get { return IsSaved && _game.SaveGame.GetBoolean(SavegameVariable); }
        }

        public bool IsEmpty
        {
            get { return String.IsNullOrEmpty(_itemName); }
        }

        public bool IsObtainable
        {
            get
            {
                return String.IsNullOrEmpty(_itemName) ||
                       _game.Equipment.GetItem(_itemName).IsObtainable;
            }
                    
        }

        public void EnsureObtainable()
        {
            if (!String.IsNullOrEmpty(_itemName) && !_game.Equipment.GetItem(_itemName).IsObtainable)
            {
                _itemName = String.Empty;
                _variant = 1;
            }
        }

        public void CheckObtainable()
        {
            if (!IsObtainable)
                Debug.Die("Treasure '{0}' is not allowed, did you call EnsureObtainable()?");
        }
    }
}
