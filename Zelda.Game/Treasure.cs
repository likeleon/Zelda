using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Treasure
    {
        public Treasure(Game game, string itemName, int variant, string savegameVariable)
        {
            _game = game;
            _itemName = itemName;
            _variant = variant;
            SavegameVariable = savegameVariable;
        }

        readonly Game _game;
        public Game Game
        {
            get { return _game; }
        }

        string _itemName;
        public string ItemName
        {
            get { return _itemName; }
        }

        public EquipmentItem Item
        {
            get { return _game.Equipment.GetItem(ItemName); }
        }

        int _variant;
        public int Variant
        {
            get { return _variant; }
        }

        public string SavegameVariable { get; private set; }

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

        public void GiveToPlayer()
        {
            Debug.CheckAssertion(!IsFound, "This treasure was already found");
            CheckObtainable();

            if (IsSaved)
                _game.SaveGame.SetBoolean(SavegameVariable, true);

            if (Item.IsSaved)
                Item.SetVariant(Variant);
        }

        Sprite _sprite;

        public void Draw(Surface dstSurface, int x, int y)
        {
            if (_sprite == null)
            {
                _sprite = new Sprite("entities/items");
                _sprite.SetCurrentAnimation(ItemName);
                _sprite.SetCurrentDirection((Direction4)(Variant - 1));
            }

            _sprite.Draw(dstSurface, x, y);
        }
    }
}
