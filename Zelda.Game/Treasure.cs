﻿using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    class Treasure
    {
        Sprite _sprite;

        public Game Game { get; }
        public string ItemName { get; private set; }
        public int Variant { get; private set; }
        public string SavegameVariable { get; }
        public EquipmentItem Item { get { return Game.Equipment.GetItem(ItemName); } }

        public bool IsSaved { get { return !SavegameVariable.IsNullOrEmpty(); } }
        public bool IsFound { get { return IsSaved && Game.SaveGame.GetBoolean(SavegameVariable); } }
        public bool IsEmpty { get { return ItemName.IsNullOrEmpty(); } }
        public bool IsObtainable { get { return ItemName.IsNullOrEmpty() || Game.Equipment.GetItem(ItemName).IsObtainable; } }

        public Treasure(Game game, string itemName, int variant, string savegameVariable)
        {
            Game = game;
            ItemName = itemName;
            Variant = variant;
            SavegameVariable = savegameVariable;
        }

        public void EnsureObtainable()
        {
            if (!ItemName.IsNullOrEmpty() && !Game.Equipment.GetItem(ItemName).IsObtainable)
            {
                ItemName = "";
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

            Item.OnObtaining(Variant, IsSaved ? SavegameVariable : null);
        }

        public void Draw(Surface dstSurface, int x, int y)
        {
            if (_sprite == null)
            {
                _sprite = Sprite.Create("entities/items", false);
                _sprite.SetCurrentAnimation(ItemName);
                _sprite.SetCurrentDirection((Direction4)(Variant - 1));
            }

            _sprite.Draw(dstSurface, x, y);
        }
    }
}
