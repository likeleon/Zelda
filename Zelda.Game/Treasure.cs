using System;

namespace Zelda.Game
{
    class Treasure
    {
        readonly Game _game;
        readonly string _itemName;
        readonly int _variant;
        readonly string _savegameVariable;

        public Treasure(Game game, string itemName, int variant, string savegameVariable)
        {
            _game = game;
            _itemName = itemName;
            _variant = variant;
            _savegameVariable = savegameVariable;
        }

        public bool IsSaved
        {
            get { return !String.IsNullOrEmpty(_savegameVariable); }
        }

        public bool IsFound
        {
            get { return IsSaved && _game.SaveGame.GetBoolean(_savegameVariable); }
        }
    }
}
