
namespace Zelda.Game
{
    class Treasure
    {
        readonly Game _game;
        readonly string _itemName;
        readonly int _variant;
        readonly string _savegameVariable;
        Sprite _sprite;

        public Treasure(Game game, string itemName, int variant, string savegameVariable)
        {
            _game = game;
            _itemName = itemName;
            _variant = variant;
            _savegameVariable = savegameVariable;
        }
    }
}
