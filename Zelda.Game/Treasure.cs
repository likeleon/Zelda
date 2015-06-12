using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Treasure
    {
        Sprite _sprite;

        public Game Game { get; private set; }
        public string ItemName { get; private set; }
        public EquipmentItem Item { get { return Game.Equipment.GetItem(ItemName); } }
        public int Variant { get; private set; }
        public string SavegameVariable { get; private set; }

        public bool IsSaved { get { return !String.IsNullOrEmpty(SavegameVariable); } }
        public bool IsFound { get { return IsSaved && Game.SaveGame.GetBoolean(SavegameVariable); } }
        public bool IsEmpty { get { return String.IsNullOrEmpty(ItemName); } }
        public bool IsObtainable
        {
            get
            {
                return String.IsNullOrEmpty(ItemName) ||
                       Game.Equipment.GetItem(ItemName).IsObtainable;
            }
        }

        public Treasure(Game game, string itemName, int variant, string savegameVariable)
        {
            Game = game;
            ItemName = itemName;
            Variant = variant;
            SavegameVariable = savegameVariable;
        }

        public void EnsureObtainable()
        {
            if (!String.IsNullOrEmpty(ItemName) && !Game.Equipment.GetItem(ItemName).IsObtainable)
            {
                ItemName = String.Empty;
                Variant = 1;
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
                Game.SaveGame.SetBoolean(SavegameVariable, true);

            if (Item.IsSaved)
                Item.SetVariant(Variant);

            Item.NotifyObtaining(this);
        }

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
